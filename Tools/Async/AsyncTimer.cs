using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Apprentice.Tools {
    public class AsyncTimer {

        private AsyncGate<bool> gate = new AsyncGate<bool>(true);
        private CancellationTokenSource cancelSource;
        public Action callback;

        public bool Running { get; private set; }
        public int Duration { get; private set; }
        public long StartTime { get; private set; }
        public long EndTime => StartTime + Duration;
        public int RemainingTime => Math.Max(0, (int) (EndTime - Time.Now));
        public int PassedTime => (int) (Time.Now - StartTime);
        /// <summary>Return progress as a percentage</summary>
        public double Progress => Duration / (double) (Time.Now - StartTime);

        public AsyncTimer(Action callback) {
            this.callback = callback ?? throw new ArgumentNullException("Callback was null");
        }

        public AsyncTimer Start(int duration) {
            if (Running)
                throw new Exception("Timer start failed: Timer is already running");
            if (duration < 0)
                throw new ArgumentException("Timer duration must be positive");
            StartTime = Time.Now;
            Duration = duration;
            cancelSource = new CancellationTokenSource();
            gate.Close();
            SetRunning(true);
            Task.Run(Run);
            return this;
        }

        /// <summary>Set the duration so that it is <paramref name="duration"/> milliseconds into the future from current time</summary>
        /// <remarks>This method effectively overwrites the previous duration to 'passed time' + 'new duration'</remarks>
        public AsyncTimer Adjust(int duration) {
            if (duration < 0)
                throw new ArgumentException("Given duration must be positive");
            Duration = PassedTime + duration;
            return this;
        }

        /// <summary>Extends the current duration by the given amount</summary>
        public AsyncTimer Extend(int duration) {
            if (Duration + duration < 0)
                throw new InvalidOperationException("Total duration cannot be set to negative");
            Duration += duration;
            return this;
        }

        /// <summary>Cancel the timer without activating the callback</summary>
        public AsyncTimer Stop() {
            if (cancelSource != null && !cancelSource.IsCancellationRequested)
                Task.Run(cancelSource.Cancel);
            SetRunning(false);
            return this;
        }

        /// <summary>Stop the timer and activate the callback immediately</summary>
        public AsyncTimer Finish() {
            Stop();
            callback.Invoke();
            return this;
        }

        public AsyncTimer Reset(int duration = 0) {
            Stop();
            Start(duration != 0 ? duration : Duration);
            return this;
        }

        public async Task<bool> Wait(int timeout = 0) {
            if (timeout != 0) {
                var job = await gate.Wait(timeout);
                return job.Result;
            } else {
                await gate.Wait();
                return true;
            }
        }

        private async void Run() {
            while (RemainingTime > 0) {
                var task = Task.Delay(RemainingTime, cancelSource.Token);

                try {
                    await task;
                } catch (TaskCanceledException) {
                    return;
                }
            }

            SetRunning(false);
            callback.Invoke();
        }

        private void SetRunning(bool state) {
            Running = state;
            if (state) {
                gate.Close();
            } else {
                gate.Open();
            }
        }
    }
}
