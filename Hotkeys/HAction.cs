using System;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Hotkeys {
    public abstract class HAction {

        /// <summary>The action is currently running</summary>
        public bool Running { get; private set; }
        /// <summary>The action has been requested to stop</summary>
        public bool IsStopping { get; private set; }
        /// <summary>The hotkey that this action is bound to</summary>
        public Hotkey Hotkey { get; private set; }
        /// <summary>Specifies if this action triggers on press or release</summary>
        public bool FireState { get; private set; }
        /// <summary>The state id of this action's main key when it was launched</summary>
        public uint StateID { get; private set; }
        /// <summary>Check if the state of the main key has changed since launch</summary>
        public bool HasChanged => KeyHandler.HasChanged(Hotkey.MainKey, StateID);

        protected abstract Task Body();
        public abstract HAction Copy();
        public virtual void Check() { }

        public async Task Start() {
            if (Hotkey == null) {
                throw new Exception("This action object's Hotkey is null");
            } else if (Running) {
                throw new Exception("This action object is already running");
            }

            Running = true;
            StateID = KeyHandler.GetStateID(Hotkey.MainKey);

            await Body();

            IsStopping = false;
            Running = false;
        }

        /// <summary>Request action to stop executing. Only works if the action chooses respond to the request.</summary>
        public virtual void Stop() => IsStopping = true;

        /// <summary>Set the hotkey owner and press/release of this action. Use only when creating a hotkey.</summary>
        public void Bind(Hotkey h, bool fireState) {
            Hotkey = h;
            FireState = fireState;
            Check();
        }

        public static Func<T, Task> ToAsync<T>(Action<T> action) => async a => await Task.Run(() => action(a));

        #region syntax helpers
        public static SingleAction Single(Action<SingleAction> action) => new SingleAction(action);
        public static SingleAction Single(Func<SingleAction, Task> action) => new SingleAction(action);

        public static RapidAction Rapid(Action<RapidAction> action) => new RapidAction(action);
        public static RapidAction Rapid(int? delay, Action<RapidAction> action) => new RapidAction(delay, action);
        public static RapidAction Rapid(int? delay, int? amount, Action<RapidAction> action) => new RapidAction(delay, amount, action);
        public static RapidAction Rapid(Func<RapidAction, Task> action) => new RapidAction(action);
        public static RapidAction Rapid(int? delay, Func<RapidAction, Task> action) => new RapidAction(delay, action);
        public static RapidAction Rapid(int? delay, int? amount, Func<RapidAction, Task> action) => new RapidAction(delay, amount, action);

        public static RepeatAction Repeat(Action<RepeatAction> action) => new RepeatAction(action);
        public static RepeatAction Repeat(Func<RepeatAction, Task> action) => new RepeatAction(action);

        public static HoldAction Hold(int timeWindow, Action<HoldAction> action, Action<HoldAction> hold) => new HoldAction(timeWindow, action, hold);
        public static HoldAction Hold(int timeWindow, Func<HoldAction, Task> action, Action<HoldAction> hold) => new HoldAction(timeWindow, action, hold);
        public static HoldAction Hold(int timeWindow, Action<HoldAction> action, Func<HoldAction, Task> hold) => new HoldAction(timeWindow, action, hold);
        public static HoldAction Hold(int timeWindow, Func<HoldAction, Task> action, Func<HoldAction, Task> hold) => new HoldAction(timeWindow, action, hold);

        public static DoubleAction Double(int timeWindow, Action<DoubleAction> action, Action<DoubleAction> secondary) => new DoubleAction(timeWindow, action, secondary);
        public static DoubleAction Double(int timeWindow, Func<DoubleAction, Task> action, Action<DoubleAction> secondary) => new DoubleAction(timeWindow, action, secondary);
        public static DoubleAction Double(int timeWindow, Action<DoubleAction> action, Func<DoubleAction, Task> secondary) => new DoubleAction(timeWindow, action, secondary);
        public static DoubleAction Double(int timeWindow, Func<DoubleAction, Task> action, Func<DoubleAction, Task> secondary) => new DoubleAction(timeWindow, action, secondary);
        #endregion
    }

    public class SingleAction : HAction {

        private Func<SingleAction, Task> Action { get; set; }

        private SingleAction(SingleAction action) : this(action.Action) { }
        public SingleAction(Action<SingleAction> action) : this(ToAsync(action)) { }
        public SingleAction(Func<SingleAction, Task> action) {
            Action = action;
        }

        protected override async Task Body() => await Action(this);
        public override HAction Copy() => new SingleAction(this);
    }

    public class RapidAction : HAction {

        private Func<RapidAction, Task> Action { get; set; }

        public int? Delay { get; set; }
        public int? Amount { get; set; }
        public int Current { get; private set; }

        private RapidAction(RapidAction other) : this(other.Delay, other.Amount, other.Action) { }

        public RapidAction(Action<RapidAction> action) : this(null, null, ToAsync(action)) { }
        public RapidAction(int? delay, Action<RapidAction> action) : this(delay, null, ToAsync(action)) { }
        public RapidAction(int? delay, int? amount, Action<RapidAction> action) : this(delay, amount, ToAsync(action)) { }

        public RapidAction(Func<RapidAction, Task> action) : this(null, null, action) { }
        public RapidAction(int? delay, Func<RapidAction, Task> action) : this(delay, null, action) { }
        public RapidAction(int? delay, int? amount, Func<RapidAction, Task> action) {
            Delay = delay;
            Amount = amount;
            Action = action;
        }

        protected override async Task Body() {
            Current = 0;

            while (!HasChanged && (Amount == null || Amount >= Current)) {
                var action = Action(this);
                if (Delay != null)
                    await Task.Delay((int) Delay);
                await action;
                Current++;
            }
        }

        public override void Check() {
            if (!FireState) {
                throw new Exception("This action type cannot be assigned to key release.");
            }
        }

        public override HAction Copy() => new RapidAction(this);
    }

    public class RepeatAction : HAction {

        private static int RepeatSpeed = 20;

        private Func<RepeatAction, Task> Action { get; set; }
        public int CurrentRepeat { get; private set; }

        private RepeatAction(RepeatAction other) : this(other.Action) { }
        public RepeatAction(Action<RepeatAction> action) : this(ToAsync(action)) { }
        public RepeatAction(Func<RepeatAction, Task> action) {
            Action = action;
        }

        protected override async Task Body() {
            CurrentRepeat = 0;
            var source = new TaskCompletionSource<object>(TaskContinuationOptions.RunContinuationsAsynchronously);
            KeyHandler.InputEvent += Cutoff;

            var t = Action(this);
            await Task.WhenAny(Task.Delay(KeyHandler.KeyRepeatDelay), source.Task);
            await t;

            while (!HasChanged && !source.Task.IsCompleted) {
                CurrentRepeat++;
                t = Action(this);
                await Task.Delay(RepeatSpeed);
                await t;
            }

            void Cutoff(Key key, bool state) {
                if (key == Hotkey.MainKey && !state) {
                    KeyHandler.InputEvent -= Cutoff;
                    source.TrySetResult(null);
                }
            }
        }

        public override void Check() {
            if (!FireState) {
                throw new Exception("This action type cannot be assigned to key release.");
            }
        }

        public override HAction Copy() => new RepeatAction(this);
    }

    public class HoldAction : HAction {

        private Func<HoldAction, Task> FQuick { get; set; }
        private Func<HoldAction, Task> FHold { get; set; }

        public int TimeWindow { get; private set; }

        private HoldAction(HoldAction other) : this(other.TimeWindow, other.FQuick, other.FHold) { }
        public HoldAction(int timeWindow, Action<HoldAction> action, Action<HoldAction> hold) : this(timeWindow, ToAsync(action), ToAsync(hold)) { }
        public HoldAction(int timeWindow, Func<HoldAction, Task> action, Action<HoldAction> hold) : this(timeWindow, action, ToAsync(hold)) { }
        public HoldAction(int timeWindow, Action<HoldAction> action, Func<HoldAction, Task> hold) : this(timeWindow, ToAsync(action), hold) { }
        public HoldAction(int timeWindow, Func<HoldAction, Task> action, Func<HoldAction, Task> hold) {
            TimeWindow = timeWindow;
            FQuick = action;
            FHold = hold;
        }

        protected override async Task Body() {
            if (await KeyHandler.WaitKeyUp(Hotkey.MainKey, TimeWindow))
                await FQuick(this);
            else
                await FHold(this);
        }

        public override void Check() {
            if (!FireState) {
                throw new Exception("This action type cannot be assigned to key release.");
            }
        }

        public override HAction Copy() => new HoldAction(this);
    }

    public class DoubleAction : HAction {

        private Func<DoubleAction, Task> FSingle { get; set; }
        private Func<DoubleAction, Task> FDouble { get; set; }

        public int TimeWindow { get; private set; }

        private DoubleAction(DoubleAction other) : this(other.TimeWindow, other.FSingle, other.FDouble) { }
        public DoubleAction(int timeWindow, Action<DoubleAction> action, Action<DoubleAction> doubleAction) : this(timeWindow, ToAsync(action), ToAsync(doubleAction)) { }
        public DoubleAction(int timeWindow, Func<DoubleAction, Task> action, Action<DoubleAction> doubleAction) : this(timeWindow, action, ToAsync(doubleAction)) { }
        public DoubleAction(int timeWindow, Action<DoubleAction> action, Func<DoubleAction, Task> doubleAction) : this(timeWindow, ToAsync(action), doubleAction) { }
        public DoubleAction(int timeWindow, Func<DoubleAction, Task> action, Func<DoubleAction, Task> doubleAction) {
            TimeWindow = timeWindow;
            FSingle = action;
            FDouble = doubleAction;
        }

        protected override async Task Body() {
            if (await KeyHandler.WaitKey(Hotkey.MainKey, TimeWindow))
                await FDouble(this);
            else
                await FSingle(this);
        }

        public override HAction Copy() => new DoubleAction(this);
    }
}
