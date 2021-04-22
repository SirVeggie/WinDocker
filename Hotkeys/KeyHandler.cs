using Apprentice.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Hotkeys {
    public class KeyHandler {

        private static readonly uint pid = (uint) Process.GetCurrentProcess().Id;

        #region properties
        public static int HistorySize { get; } = 100;

        public static DropStack<KeyState> History { get; private set; } = new DropStack<KeyState>(HistorySize);
        public static HashSet<Key> DownKeys { get; private set; } = new HashSet<Key>();
        public static HashSet<Key> DownKeysVirtual { get; private set; } = new HashSet<Key>();
        public static Dictionary<Key, uint> StateIDs { get; private set; } = new Dictionary<Key, uint>();
        public static Dictionary<Key, uint> GlobalIDs { get; private set; } = new Dictionary<Key, uint>();
        public static uint CurrentGlobalID { get; private set; } = 0;

        public static long Idle => Math.Min(KeyIdle, MouseIdle);
        public static long KeyIdle => Time.Now - LastKeyboardAction;
        public static long MouseIdle => Time.Now - LastMouseAction;
        public static long LastKeyboardAction { get; private set; }
        public static long LastMouseAction { get; private set; }

        /// <summary>Returns the amount of keys currently held down physically.</summary>
        public static int KeyCount => DownKeys.Count;
        /// <summary>Returns the amount of keys currently held down virtually.</summary>
        public static int KeyCountVirtual => DownKeysVirtual.Count;

        /// <summary>Subscribe to input events. Keep callbacks VERY short or async.</summary>
        public static event Action<Key, bool> InputEvent;
        /// <summary>Subscribe to mouse move events. ALWAYS make callbacks instant or async.</summary>
        public static event Action MoveEvent;

        public static int KeyRepeatDelay { get; set; } = (System.Windows.Forms.SystemInformation.KeyboardDelay + 1) * 250;
        public static int KeyRepeatSpeed { get; set; } = (int) Matht.ToRange(System.Windows.Forms.SystemInformation.KeyboardSpeed, 0, 31, 1000 / 2.5, 1000 / 30.0);

        /// <summary>Regards the numlock and non-numlock variants as the same key</summary>
        public static bool StatelessNumpad { get; set; }

        #region locking
        /// <summary>All user input is locked</summary>
        public static bool IsLocked => lockCount > 0;
        private static int lockCount = 0;
        /// <summary>All mouse input is locked</summary>
        public static bool IsMouseLocked => mouseLockCount > 0;
        private static int mouseLockCount = 0;
        /// <summary>All keyboard input is locked</summary>
        public static bool IsKeyboardLocked => keyboardLockCount > 0;
        private static int keyboardLockCount = 0;
        /// <summary>All user input except mouse movement is locked</summary>
        public static bool IsAllKeysLocked => allKeysLockCount > 0;
        private static int allKeysLockCount = 0;
        /// <summary>Mouse movement is locked</summary>
        public static bool IsMouseMoveLocked => mouseMoveLockCount > 0;
        private static int mouseMoveLockCount = 0;

        public static Dictionary<Key, int> LockedKeys { get; private set; } = new Dictionary<Key, int>();
        #endregion

        #endregion

        #region input
        public static void StartInput() {
            DeviceHook.InputEvent += DeviceInput;
            DeviceHook.StartHooks();
        }

        public static bool DeviceInput(IDeviceInput input) {
            // Mouse move is handled here to reduce performance impact due to its high frequency
            if (input.Key == Key.MouseMove) {
                if (input.Injected)
                    return false;
                MoveEvent?.Invoke();
                LastMouseAction = Time.Now;
                return IsLocked || IsMouseLocked || IsMouseMoveLocked;
            }

            if (input.Key.IsUnknown())
                return false;
            var key = !StatelessNumpad || !input.Key.IsNumpad() ? input.Key : MapNumpad(input.Key);

            // Deal with injected input
            if (input.Injected && (!key.IsMedia() || (uint) input.ExtraInfo == pid)) {
                if (!key.IsStateless()) {
                    if (input.State)
                        DownKeysVirtual.Add(key);
                    else
                        DownKeysVirtual.Remove(key);
                }

                return false;
            }

            // Record key state if it has changed (ignore auto-repeat presses)
            if (input.State != IsDown(key)) {
                if (!key.IsStateless())
                    SetState(key, input.State);
                History.Push(new KeyState(key, input.State));
                GlobalIDs[key] = ++CurrentGlobalID;
            }

            // Invoke special key event subscriptions
            InputEvent?.Invoke(key, input.State);

            // Check lock state
            bool locked = IsLocked || IsAllKeysLocked || (key.IsMouse() ? IsMouseLocked : IsKeyboardLocked) || IsLockedKey(key);
            bool blocked;

            // Update idle info
            if (key.IsMouse())
                LastMouseAction = Time.Now;
            else
                LastKeyboardAction = Time.Now;

            // If hotkey is running tell it that the key has been released
            if (!input.State && HotkeyManager.CurrentHotkeys.ContainsKey(key))
                blocked = HotkeyManager.KeyUp(key);

            // Block key if locked
            else if (locked)
                return true;

            // Hotkey for that key is already running
            else if (HotkeyManager.CurrentHotkeys.ContainsKey(key))
                blocked = HotkeyManager.IsBlocked(key);

            // Pass key event to hotkey manager
            else if (input.State)
                blocked = HotkeyManager.KeyDown(key);
            else
                blocked = HotkeyManager.KeyUp(key);

            if (!blocked)
                SetStateVirtual(key, input.State);
            return blocked;
        }

        public static Key MapNumpad(Key key) {
            switch (key) {
            case Key.NumpadIns:
                key = Key.Numpad0;
                break;
            case Key.NumpadEnd:
                key = Key.Numpad1;
                break;
            case Key.NumpadDown:
                key = Key.Numpad2;
                break;
            case Key.NumpadPgDn:
                key = Key.Numpad3;
                break;
            case Key.NumpadLeft:
                key = Key.Numpad4;
                break;
            case Key.NumpadClear:
                key = Key.Numpad5;
                break;
            case Key.NumpadRight:
                key = Key.Numpad6;
                break;
            case Key.NumpadHome:
                key = Key.Numpad7;
                break;
            case Key.NumpadUp:
                key = Key.Numpad8;
                break;
            case Key.NumpadPgUp:
                key = Key.Numpad9;
                break;
            case Key.NumpadDel:
                key = Key.NumpadDot;
                break;
            default:
                break;
            }

            return key;
        }
        #endregion

        #region states
        public static bool IsDown(Key key) {
            if (key.IsModifierFlag()) {
                if (key.IsShift()) {
                    return IsDown(Key.LShift) || IsDown(Key.RShift);
                } else if (key.IsCtrl()) {
                    return IsDown(Key.LCtrl) || IsDown(Key.RCtrl);
                } else if (key.IsAlt()) {
                    return IsDown(Key.LAlt) || IsDown(Key.RAlt);
                } else if (key.IsWin()) {
                    return IsDown(Key.LWin) || IsDown(Key.RWin);
                }
            }

            return DownKeys.Contains(key);
        }

        public static bool IsDownVirtual(Key key) {
            if (key.IsModifierFlag()) {
                if (key.IsShift()) {
                    return IsDownVirtual(Key.LShift) || IsDownVirtual(Key.RShift);
                } else if (key.IsCtrl()) {
                    return IsDownVirtual(Key.LCtrl) || IsDownVirtual(Key.RCtrl);
                } else if (key.IsAlt()) {
                    return IsDownVirtual(Key.LAlt) || IsDownVirtual(Key.RAlt);
                } else if (key.IsWin()) {
                    return IsDownVirtual(Key.LWin) || IsDownVirtual(Key.RWin);
                }
            }

            return DownKeysVirtual.Contains(key);
        }

        private static void SetState(Key key, bool state) {
            if (StateIDs.ContainsKey(key)) {
                StateIDs[key]++;
            } else {
                StateIDs.Add(key, 1);
            }

            if (state) {
                DownKeys.Add(key);
            } else {
                DownKeys.Remove(key);
            }
        }

        private static void SetStateVirtual(Key key, bool state) {
            if (key.IsStateless())
                return;
            if (state) {
                DownKeysVirtual.Add(key);
            } else {
                DownKeysVirtual.Remove(key);
            }
        }

        public static uint GetStateID(Key key) {
            if (!StateIDs.ContainsKey(key)) {
                return 0;
            } else {
                return StateIDs[key];
            }
        }

        public static bool HasChanged(Key key, uint stateID) {
            return GetStateID(key) > stateID;
        }

        public static void ResetDownVirtual() => DownKeysVirtual = new HashSet<Key>();

        public static Key[] DisableModifiers() {
            List<Key> list = new List<Key>();

            foreach (var key in DownKeysVirtual) {
                if (key.IsModifier()) {
                    list.Add(key);
                }
            }

            Key[] keys = list.ToArray();
            Input.SendUp(keys);
            return keys;
        }

        public static void EnableModifiers(Key[] keys) {
            Input.SendDown(keys);
        }
        #endregion

        #region locking
        /// <summary>Lock all input.</summary>
        public static void Lock(bool state) {
            if (!state && lockCount < 1)
                throw new Exception("System is already unlocked");
            lockCount += state ? 1 : -1;
        }

        /// <summary>Lock all mouse input.</summary>
        public static void LockMouse(bool state) {
            if (!state && mouseLockCount < 1)
                throw new Exception("Mouse is already unlocked");
            mouseLockCount += state ? 1 : -1;
        }

        /// <summary>Lock all keyboard input.</summary>
        public static void LockKeyboard(bool state) {
            if (!state && keyboardLockCount < 1)
                throw new Exception("Keyboard is already unlocked");
            keyboardLockCount += state ? 1 : -1;
        }

        /// <summary>Lock all keys. Doesn't include mouse move.</summary>
        public static void LockAllKeys(bool state) {
            if (!state && allKeysLockCount < 1)
                throw new Exception("All keys lock state is already unlocked");
            allKeysLockCount += state ? 1 : -1;
        }

        /// <summary>Lock mouse move.</summary>
        public static void LockMouseMove(bool state) {
            if (!state && mouseMoveLockCount < 1)
                throw new Exception("Mouse move is already unlocked");
            mouseMoveLockCount += state ? 1 : -1;
        }

        /// <summary>Lock specific keys.</summary>
        public static void LockKey(params Key[] keys) {
            foreach (Key key in keys) {
                if (!LockedKeys.ContainsKey(key)) {
                    LockedKeys.Add(key, 1);
                } else {
                    LockedKeys[key]++;
                }
            }
        }

        /// <summary>Unlock specific keys that have been locked by <see cref="LockKey(Key[])"/> before.</summary>
        public static void UnlockKey(params Key[] keys) {
            foreach (Key key in keys) {
                if (!LockedKeys.ContainsKey(key) || LockedKeys[key] == 0) {
                    throw new ArgumentException("Key " + key + " is already unlocked");
                }

                LockedKeys[key]--;
            }
        }

        public static void ResetLockedKeys() {
            LockedKeys = new Dictionary<Key, int>();
        }

        public static bool IsLockedKey(Key key) {
            if (!LockedKeys.ContainsKey(key)) {
                return false;
            } else {
                return LockedKeys[key] > 0;
            }
        }
        #endregion

        #region keywait
        public static async Task<Job<KeyState>> WaitKey(Func<Key, bool, bool> predicate, int? timeout = null) {
            if (predicate == null)
                throw new ArgumentNullException("Predicate was null");
            var job = new TaskCompletionSource<KeyState>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Filter(Key key, bool state) {
                if (predicate.Invoke(key, state)) {
                    InputEvent -= Filter;
                    job.TrySetResult(new KeyState(key, state));
                }
            }

            InputEvent += Filter;
            var res = await AsyncTool.Timeout(job.Task, timeout);
            return res;
        }

        /// <summary>Wait for the <paramref name="key"/> to be in a down position</summary>
        /// <param name="timeout">Time until waiting is canceled</param>
        /// <param name="block">Block <paramref name="key"/> from the OS while waiting</param>
        public static async Task<bool> WaitKeyDown(Key key, int? timeout = null, bool block = false) {
            if (IsDown(key)) {
                return true;
            }

            var job = new TaskCompletionSource<object>();

            void Filter(Key k, bool s) {
                if (s && k == key) {
                    InputEvent -= Filter;
                    job.TrySetResult(null);
                }
            }

            if (block) LockKey(key);
            InputEvent += Filter;
            var res = await AsyncTool.Timeout(job.Task, timeout);
            if (block) UnlockKey(key);

            return res.Success;
        }

        /// <summary>Wait for the <paramref name="key"/> to be in an up position</summary>
        /// <param name="timeout">Time until waiting is canceled</param>
        /// <param name="block">Block <paramref name="key"/> from the OS while waiting</param>
        public static async Task<bool> WaitKeyUp(Key key, int? timeout = null, bool block = false) {
            if (!IsDown(key)) {
                return true;
            }

            var job = new TaskCompletionSource<object>();

            void Filter(Key k, bool s) {
                if (!s && k == key) {
                    InputEvent -= Filter;
                    job.TrySetResult(null);
                }
            }

            if (block) LockKey(key);
            InputEvent += Filter;
            var res = await AsyncTool.Timeout(job.Task, timeout);
            if (block) UnlockKey(key);

            return res.Success;
        }

        /// <summary>Wait for the <paramref name="key"/> to be pressed. Needs a fresh press.</summary>
        /// <param name="timeout">Time until waiting is canceled</param>
        /// <param name="block">Block <paramref name="key"/> from the OS while waiting</param>
        public static async Task<bool> WaitKey(Key key, int? timeout = null, bool block = false) {
            if (IsDown(key)) {
                var timer = Stopwatch.StartNew();
                bool success = await WaitKeyUp(key, timeout, block);

                if (!success) {
                    return false;
                }

                return await WaitKeyDown(key, timeout - (int) timer.ElapsedMilliseconds, block);
            } else {
                return await WaitKeyDown(key, timeout, block);
            }
        }

        /// <summary>Wait for the <paramref name="key"/> to switch state</summary>
        /// <param name="timeout">Time until waiting is canceled</param>
        /// <param name="block">Block <paramref name="key"/> from the OS while waiting</param>
        public static async Task<bool> WaitKeySwitch(Key key, int? timeout = null, bool block = false) {
            if (IsDown(key)) {
                return await WaitKeyUp(key, timeout, block);
            } else {
                return await WaitKeyDown(key, timeout, block);
            }
        }


        /// <summary>Wait for one of the listed keys to be pressed</summary>
        /// <param name="keys">Array of keys to listen for</param>
        /// <param name="timeout">Time until waiting is canceled</param>
        /// <param name="block">Block listed keys from the OS while waiting</param>
        public static async Task<Key> WaitKeyDown(Key[] keys, int? timeout = null, bool block = false) {
            var job = new TaskCompletionSource<Key>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Filter(Key k, bool s) {
                if (s && keys.Contains(k)) {
                    InputEvent -= Filter;
                    job.SetResult(k);
                }
            }

            if (block) LockKey(keys);
            InputEvent += Filter;
            var res = await AsyncTool.Timeout(job.Task, timeout);
            if (block) UnlockKey(keys);

            return res.Success ? res.Result : Key.None;
        }

        /// <summary>Wait for one of the listed keys to be released</summary>
        /// <param name="keys">Array of keys to listen for</param>
        /// <param name="timeout">Time until waiting is canceled</param>
        /// <param name="block">Block listed keys from the OS while waiting</param>
        public static async Task<Key> WaitKeyUp(Key[] keys, int? timeout = null, bool block = false) {
            var job = new TaskCompletionSource<Key>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Filter(Key k, bool s) {
                if (!s && keys.Contains(k)) {
                    InputEvent -= Filter;
                    job.SetResult(k);
                }
            }

            if (block) LockKey(keys);
            InputEvent += Filter;
            var res = await AsyncTool.Timeout(job.Task, timeout);
            if (block) UnlockKey(keys);

            return res.Success ? res.Result : Key.None;
        }

        /// <summary>Wait for an event of one of the listed keys</summary>
        /// <param name="keys">Array of keys to listen for</param>
        /// <param name="timeout">Time until waiting is canceled</param>
        /// <param name="block">Block listed keys from the OS while waiting</param>
        public static async Task<Key> WaitKey(Key[] keys, int? timeout = null, bool block = false) {
            var job = new TaskCompletionSource<Key>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Filter(Key k, bool s) {
                if (keys.Contains(k)) {
                    InputEvent -= Filter;
                    job.SetResult(k);
                }
            }

            if (block) LockKey(keys);
            InputEvent += Filter;
            var res = await AsyncTool.Timeout(job.Task, timeout);
            if (block) UnlockKey(keys);

            return res.Success ? res.Result : Key.None;
        }


        /// <summary>Wait for any key to be pressed</summary>
        /// <param name="timeout">Time until waiting is canceled</param>
        /// <param name="block">Block this all keys from the OS while waiting</param>
        public static async Task<Key> WaitKeyDown(int? timeout = null, bool block = false) {
            var job = new TaskCompletionSource<Key>();

            void Filter(Key k, bool s) {
                if (s) {
                    InputEvent -= Filter;
                    job.TrySetResult(k);
                }
            }

            if (block) LockAllKeys(true);
            InputEvent += Filter;
            var res = await AsyncTool.Timeout(job.Task, timeout);
            if (block) LockAllKeys(false);

            return res.Success ? res.Result : Key.None;
        }

        /// <summary>Wait for any key to be released</summary>
        /// <param name="timeout">Time until waiting is canceled</param>
        /// <param name="block">Block all keys from the OS while waiting</param>
        public static async Task<Key> WaitKeyUp(int? timeout = null, bool block = false) {
            var job = new TaskCompletionSource<Key>();

            void Filter(Key k, bool s) {
                if (!s) {
                    InputEvent -= Filter;
                    job.TrySetResult(k);
                }
            }

            if (block) LockAllKeys(true);
            InputEvent += Filter;
            var res = await AsyncTool.Timeout(job.Task, timeout);
            if (block) LockAllKeys(false);

            return res.Success ? res.Result : Key.None;
        }

        /// <summary>Wait for any key to trigger. Both up and down events count.</summary>
        /// <param name="timeout">Time until waiting is canceled</param>
        /// <param name="block">Block all keys from the OS while waiting</param>
        public static async Task<Key> WaitKeySwitch(int? timeout = null, bool block = false) {
            var job = new TaskCompletionSource<Key>();

            void Filter(Key k, bool s) {
                InputEvent -= Filter;
                job.TrySetResult(k);
            }

            if (block) LockAllKeys(true);
            InputEvent += Filter;
            var res = await AsyncTool.Timeout(job.Task, timeout);
            if (block) LockAllKeys(false);

            return res.Success ? res.Result : Key.None;
        }
        #endregion

        #region idlewait
        /// <summary>Wait until the user is no longer idle.</summary>
        public static async Task<bool> WaitForAwake(int? timeout = null) {
            var job = new TaskCompletionSource<object>();
            void e(Key k, bool s) => job.TrySetResult(null);

            InputEvent += e;
            var res = await AsyncTool.Timeout(job.Task, timeout);
            InputEvent -= e;

            return res.Success;
        }

        /// <summary>Wait for a keyboard event.</summary>
        public static async Task<bool> WaitForKeyboard(int? timeout = null) {
            var job = new TaskCompletionSource<object>();
            void e(Key k, bool s) {
                if (!k.IsMouse()) job.TrySetResult(null);
            }

            InputEvent += e;
            var res = await AsyncTool.Timeout(job.Task, timeout);
            InputEvent -= e;

            return res.Success;
        }

        /// <summary>Wait for a mouse event.</summary>
        public static async Task<bool> WaitForMouse(int? timeout = null) {
            var job = new TaskCompletionSource<object>();
            void e(Key k, bool s) {
                if (k.IsMouse()) job.TrySetResult(null);
            }

            InputEvent += e;
            var res = await AsyncTool.Timeout(job.Task, timeout);
            InputEvent -= e;

            return res.Success;
        }
        #endregion
    }
}
