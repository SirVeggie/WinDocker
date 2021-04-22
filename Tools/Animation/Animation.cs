using Apprentice.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprentice.Tools {

    public interface IAnimation {
        uint ID { get; }
        bool Running { get; }
        double Progress { get; }
        bool Completed { get; }
        object Identity { get; }

        void Start();
        void Stop();
        void Continue();
        void Skip();
        void Cancel();
        Task<bool> WaitForStop();
        IAnimation Copy();
    }

    public class Animation : IAnimation {

        public static uint CurrentID { get; private set; }
        public static int AnimationDelay { get; set; } = 5;

        protected uint state;
        protected readonly AsyncGate<bool> gate = new AsyncGate<bool>(true);
        protected readonly Stopwatch timer = new Stopwatch();

        public uint ID { get; } = ++CurrentID;
        public bool Running { get => state % 2 == 1; set => state += Running == value ? 0 : 1; }
        public double Progress => Completed ? 1 : Math.Min(1, timer.ElapsedMilliseconds / (double) Duration);
        public bool Completed { get; protected set; }
        public object Identity { get; }

        public int Duration { get; protected set; }
        public Action<double> AnimationCallback { get; protected set; }
        public Action CancelCallback { get; protected set; }

        public Animation(int duration, Action<double> action, Action cancel = null, object identity = null) {
            Duration = duration;
            AnimationCallback = action;
            CancelCallback = cancel;
            Identity = identity;
        }

        #region static methods
        public static Animation Start(int duration, Action<double> action, Action cancel = null, object identity = null) => Start(duration, null, action, cancel, identity);
        public static Animation Start(int duration, Curve curve, Action<double> action, Action cancel = null, object identity = null) {
            Animation animation;
            if (curve != null)
                animation = new Animation(duration, t => action.Invoke(curve.Invoke(t)), cancel, identity);
            else
                animation = new Animation(duration, action, cancel, identity);
            animation.Start();
            return animation;
        }
        #endregion

        #region commands
        public virtual void Start() {
            if (Running)
                throw new InvalidOperationException("Cannot start: Animation is already running");
            BaseStart();
            Completed = false;
            Loop().Throw();
        }

        public virtual void Continue() {
            if (Running)
                throw new InvalidOperationException("Cannot continue: Animation is already running");
            if (Completed || Progress == 0)
                throw new InvalidOperationException("Cannot continue: Animation hasn't started or it has already completed");
            BaseStart();
            Loop().Throw();
        }

        public virtual void Stop() {
            if (!Running)
                return;
            BaseStop(false);
        }

        public virtual void Cancel() {
            if (Progress == 0)
                return;
            timer.Reset();
            CancelCallback.Invoke();
            BaseStop(false);
        }

        public virtual void Skip() {
            if (Completed)
                return;
            AnimationCallback.Invoke(1);
            BaseStop(true);
        }

        public Task<bool> WaitForStop() => gate.Wait();

        public virtual IAnimation Copy() => new Animation(Duration, AnimationCallback, CancelCallback);
        #endregion

        #region logic
        protected virtual void BaseStart() {
            if (Identity != null)
                AnimationManager.Add(Identity, this);
            gate.Close();
            Running = true;
            timer.Start();
        }

        protected virtual void BaseStop(bool completed) {
            Running = false;
            Completed = completed;
            timer.Stop();
            if (Identity != null)
                AnimationManager.Remove(Identity, this);
            gate.Open(completed);
        }

        protected virtual async Task Loop() {
            var localState = state;

            while (Progress < 1) {
                AnimationCallback.Invoke(Progress);
                await Task.Delay(AnimationDelay).ConfigureAwait(false);
                if (localState != state)
                    return;
            }

            AnimationCallback.Invoke(1);
            BaseStop(true);
        }
        #endregion

        #region operators
        public static bool operator ==(Animation a, Animation b) => (a is null && b is null) || !(a is null) && !(b is null) && a.ID == b.ID;
        public static bool operator !=(Animation a, Animation b) => !(a == b);
        public override bool Equals(object obj) => obj is Animation animation && this == animation;
        public override int GetHashCode() => 1213502048 + ID.GetHashCode();
        #endregion
    }
}
