using Apprentice.Debugging;
using Apprentice.GUI;
using Apprentice.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Hotkeys {

    public class Hotkey {

        public static uint LatestID { get; private set; }

        #region properties
        public uint ID { get; } = ++LatestID;
        public Key MainKey { get; }
        public List<Key> Modifiers { get; }
        private List<Key> negativeModifiers;

        #region actions
        private HAction downAction;
        public HAction DownAction {
            get => downAction;
            set {
                if (value != null) {
                    var copy = value.Copy();
                    copy.Bind(this, true);
                    downAction = copy;
                }
            }
        }

        private HAction upAction;
        public HAction UpAction {
            get => upAction;
            set {
                if (value != null) {
                    var copy = value.Copy();
                    copy.Bind(this, false);
                    upAction = copy;
                }
            }
        }
        #endregion

        public Func<bool> Context { get; }

        /// <summary>Hotkey is ready to activate (is enabled, modifiers and context match)</summary>
        public bool IsActive => Enabled && ModifiersMatch() && (Context?.Invoke() ?? true);
        /// <summary>Hotkey has been activated but hasn't been released yet</summary>
        public bool IsDown { get; private set; }
        /// <summary>Specifies which duplicate of the same keyed hotkey is chosen to run, bigger is better</summary>
        public int Priority { get; }
        /// <summary>Specifies which hotkey in the same Priority bracket is chosen based on number of modifiers and the existence of a context</summary>
        public int SubPriority { get; }
        /// <summary></summary>
        public bool IsRunning => runningCount > 0;

        /// <summary>Hotkey will block the main key from passing through to windows</summary>
        public bool Block { get; set; }
        /// <summary>Hotkey is enabled</summary>
        public bool Enabled { get; set; }
        /// <summary>Hotkey doesn't require for default Windows modifiers to match exactly to its specified modifiers</summary>
        public bool Wild { get; set; }
        /// <summary>Allows for new presses of the hotkey to activate even if the previous activation hasn't ended</summary>
        public bool AllowParallel { get; set; }

        private int runningCount = 0;
        private TaskCompletionSource<object> stopAwaiter;
        private bool denyExecution;
        #endregion

        #region creation
        /// <summary>Create a new hotkey. It is automatically activated and added to the Hotkey Manager.</summary>
        /// <param name="keys">Main key and its required modifiers that trigger the hotkey</param>
        /// <param name="press">Action performed when the main key is pressed</param>
        /// <param name="context">Hotkey is active when context returns true</param>
        /// <param name="release">Action performed when the main key is released</param>
        /// <param name="priority">Higher priority hotkeys take precedence if multiple valid hotkeys are available</param>
        /// <param name="block">Prevent applications from receiving the keypress</param>
        /// <param name="wild">Ignores extra modifiers. If false, extra modifiers (ctrl, shift, alt, win) will prevent activation if pressed down.</param>
        /// <param name="parallel">Hotkey can run again even if the previous run hasn't finished yet</param>
        public static Hotkey Create(TriggerKey keys, HAction press = null, Func<bool> context = null, HAction release = null, int priority = 0, bool block = true, bool wild = true, bool parallel = false) {
            Hotkey h = new Hotkey(keys, priority, block, wild, parallel, context);

            if (press == null && release == null) {
                throw new ArgumentException("Action and Release cannot both be null");
            } else if (h.MainKey.IsStateless()) {
                if (release != null) {
                    throw new Exception("A stateless hotkey cannot have a release action");
                } else if (press.GetType() != typeof(SingleAction)) {
                    throw new Exception("A stateless hotkey must be single action");
                }
            }

            h.DownAction = press;
            h.UpAction = release;

            HotkeyManager.AddHotkey(h);
            return h;
        }

        private Hotkey(TriggerKey keys, int priority, bool block, bool wild, bool parallel, Func<bool> context) {
            MainKey = keys.MainKey;
            Modifiers = keys.Modifiers;
            Priority = priority;
            SubPriority = Math.Min(keys.Modifiers.Count, 4) + (context == null ? 0 : 5);

            Context = context;
            Block = block;
            Wild = wild;
            AllowParallel = parallel;

            Enabled = true;
            SetupNegativeModifiers();
        }
        #endregion

        #region keypress processing
        public async void KeyDown() {
            FixModifiers();

            if (denyExecution = !AllowParallel && IsRunning) {
                return;
            }

            if (IsDown)
                throw new Exception("Hotkey down running again when already down");
            if (!MainKey.IsStateless())
                IsDown = true;
            RunningStarted();

            if (DownAction != null) {
                if (DownAction.Running)
                    Branch(true);
                try {
                    await DownAction.Start();
                } catch (Exception e) {
                    HotkeyManager.ExceptionHandler?.Invoke(this, e);
                }
            }

            RunningStopped();
        }

        public async void KeyUp() {
            if (denyExecution) {
                return;
            }

            if (MainKey.IsStateless()) {
                throw new Exception("A stateless hotkey shouldn't be able to release key");
            }

            if (!AllowParallel) {
                await WaitForStop();
            }

            if (!IsDown)
                throw new Exception("Hotkey up running again when already up");
            IsDown = false;
            RunningStarted();

            if (UpAction != null) {
                if (UpAction.Running)
                    Branch(false);
                UpAction.Stop();

                try {
                    await UpAction.Start();
                } catch (Exception e) {
                    Console.WriteLine("\n----------------------");
                    Console.WriteLine($"\nError releasing hotkey {MainKey} with modifiers {Logger.Stringify(Modifiers.ToArray())}");
                    Console.WriteLine($"\n{e}\n----------------------\n");
                }
            }

            RunningStopped();
        }

        private void Branch(bool dir) {
            // This is stupid
            if (dir) {
                DownAction = DownAction;
            } else {
                UpAction = UpAction;
            }
        }
        #endregion

        #region methods
        public bool Remove() => HotkeyManager.RemoveHotkey(this);
        public bool TryRemove() {
            try {
                return HotkeyManager.RemoveHotkey(this);
            } catch {
                return false;
            }
        }

        public Task WaitForStop() {
            if (stopAwaiter == null)
                return Task.CompletedTask;
            return stopAwaiter.Task;
        }
        #endregion

        #region helpers
        private void RunningStarted() {
            if (runningCount == 0)
                stopAwaiter = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            runningCount++;
        }

        private void RunningStopped() {
            runningCount--;
            if (runningCount == 0)
                stopAwaiter.SetResult(null);
        }

        /// <summary>Prevent Win or Alt key activation if a hotkey was activated with either as a modifier.</summary>
        private void FixModifiers() {
            foreach (Key key in Modifiers) {
                if (key.IsWin() || key.IsAlt()) {
                    Input.Send(Key.NoMapping);
                    return;
                }
            }
        }

        private bool ModifiersMatch() {
            if (Modifiers != null) {
                foreach (Key key in Modifiers) {
                    if (!KeyHandler.IsDown(key)) {
                        return false;
                    }
                }
            }

            if (!Wild) {
                foreach (Key key in negativeModifiers) {
                    if (KeyHandler.IsDown(key)) {
                        return false;
                    }
                }
            }

            return true;
        }

        private void SetupNegativeModifiers() {
            HashSet<Key> allMods = new HashSet<Key> { Key.LShift, Key.RShift, Key.LCtrl, Key.RCtrl, Key.LAlt, Key.RAlt, Key.LWin, Key.RWin };
            HashSet<Key> negation = new HashSet<Key>();

            if (Modifiers == null) {
                negativeModifiers = allMods.ToList();
                return;
            }

            foreach (Key key in Modifiers) {
                if (key.IsModifier() && !key.IsModifierFlag()) {
                    negation.Add(key);
                } else if (key.IsShift()) {
                    negation.Add(Key.LShift);
                    negation.Add(Key.RShift);
                } else if (key.IsCtrl()) {
                    negation.Add(Key.LCtrl);
                    negation.Add(Key.RCtrl);
                } else if (key.IsAlt()) {
                    negation.Add(Key.LAlt);
                    negation.Add(Key.RAlt);
                } else if (key.IsWin()) {
                    negation.Add(Key.LWin);
                    negation.Add(Key.RWin);
                }
            }

            negativeModifiers = allMods.Except(negation).ToList();
        }

        public override string ToString() => "{ Hotkey: " + MainKey + " | Modifiers: " + Logger.Stringify(Modifiers) + " | Priority: " + Priority + "}";
        #endregion
    }
}
