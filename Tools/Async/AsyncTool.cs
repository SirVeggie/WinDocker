using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Apprentice.Tools {
    public static class AsyncTool {

        public static TaskCompletionSource<object> NewSource() => new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        public static TaskCompletionSource<T> NewSource<T>() => new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>Checks for the <paramref name="condition"/> repeatedly until it is true.</summary>
        /// <param name="checkDelay">The time period between checks.</param>
        /// <param name="timeout">Time until the job is canceled.</param>
        public static async Task<bool> WaitForCondition(Func<bool> condition, int checkDelay, int? timeout) {
            if (condition == null) {
                throw new ArgumentException("Condition function can't be null");
            }

            Stopwatch watch = null;
            if (timeout != null)
                watch = Stopwatch.StartNew();
            while (timeout == null || watch.ElapsedMilliseconds < timeout) {
                if (condition.Invoke())
                    return true;
                await Task.Delay(checkDelay);
            }

            return false;
        }

        public static async Task<bool> WaitForEvent(Action<Action> sub, Action<Action> unsub, int? timeout = null) {
            var job = new TaskCompletionSource<object>();
            Action f = () => job.TrySetResult(null);

            return await WaitForEventBase(job.Task, f, sub, unsub, timeout);
        }

        private static async Task<bool> WaitForEventBase<T>(Task task, T action, Action<T> sub, Action<T> unsub, int? timeout) {
            sub.Invoke(action);

            if (timeout == null) {
                await task;
                unsub.Invoke(action);
                return true;
            } else if (timeout > int.MaxValue) {
                throw new ArgumentException("Timeout value is too large");
            }

            var cancel = new CancellationTokenSource();
            var delay = Task.Delay((int) timeout, cancel.Token).ContinueWith(result => { var e = result.Exception; }, TaskContinuationOptions.ExecuteSynchronously);
            var res = await Task.WhenAny(task, delay);
            unsub.Invoke(action);

            if (res == delay) {
                return false;
            } else {
                cancel.Cancel();
                return true;
            }
        }

        public static async Task<Job<T>> Timeout<T>(Task<T> task, int? timeout = null) {
            if (timeout == null) {
                return new Job<T>(await task);
            }

            var cancel = new CancellationTokenSource();

            // Swallow cancel exception with continuation
            var delay = Task.Delay((int) timeout, cancel.Token).ContinueWith(result => { var e = result.Exception; }, TaskContinuationOptions.ExecuteSynchronously);
            var res = await Task.WhenAny(task, delay);

            if (res == delay) {
                return Job.Failed();
            } else {
                cancel.Cancel();
                return new Job<T>(((Task<T>) res).Result);
            }
        }
    }
}
