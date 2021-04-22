using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Apprentice.Tools {

    /// <summary>Async flow controller that supports multiple threads</summary>
    public abstract class AsyncGateBase {

        protected const int waitIndefinitely = -1;
        protected volatile TaskCompletionSource<object> source = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool IsOpen { get; private set; }

        public AsyncGateBase() : this(false) { }
        /// <param name="open">Set the initial state of the gate to open or closed</param>
        public AsyncGateBase(bool open) {
            if (open) {
                source.TrySetResult(null);
            }
        }

        /// <summary>Open the gate, all awaits will resume immediately</summary>
        public void Open() {
            IsOpen = true;
            source.TrySetResult(null);
        }

        /// <summary>Close the gate, all awaits will wait until the gate is opened</summary>
        public void Close() {
            IsOpen = false;
            var current = source;
            if (!current.Task.IsCompleted)
                return;
            Interlocked.CompareExchange(ref source, new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously), current);
        }

        /// <summary>Let all current awaiters through the gate. Does not affect open state.</summary>
        public void Release() {
            if (IsOpen)
                return;
            Open();
            Close();
        }

        protected async Task<bool> AwaitCompletion(int timeout, CancellationToken token) {
            if (timeout < 0 && timeout != waitIndefinitely)
                throw new ArgumentException("Timeout value cannot be negative");
            CancellationTokenSource timeoutSource = null;

            if (!token.CanBeCanceled) {
                if (timeout == waitIndefinitely) {
                    await source.Task;
                    return true;
                }

                timeoutSource = new CancellationTokenSource();
            } else {
                timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            }

            using (timeoutSource) {
                Task delayTask = Task.Delay(timeout, timeoutSource.Token).ContinueWith(result => { var e = result.Exception; }, TaskContinuationOptions.ExecuteSynchronously);

                var resultTask = await Task.WhenAny(source.Task, delayTask).ConfigureAwait(false);

                if (resultTask != delayTask) {
                    timeoutSource.Cancel();
                    return true;
                }

                token.ThrowIfCancellationRequested();
                return false;
            }
        }
    }

    public sealed class AsyncGate : AsyncGateBase {

        public AsyncGate() : base(false) { }
        /// <param name="open">Set the initial state of the gate to open or closed</param>
        public AsyncGate(bool open) : base(open: open) { }

        /// <summary>Wait for the gate to open</summary>
        public Task Wait() => AwaitCompletion(waitIndefinitely, default);
        /// <summary>Wait for the gate to open</summary>
        public Task Wait(CancellationToken token) => AwaitCompletion(waitIndefinitely, token);
        /// <summary>Wait for the gate to open</summary>
        public Task<bool> Wait(int timeout, CancellationToken token) => AwaitCompletion(timeout, token);
        /// <summary>Wait for the gate to open</summary>
        public Task<bool> Wait(int timeout) => AwaitCompletion(timeout, default);
    }

    public sealed class AsyncGate<T> : AsyncGateBase {

        private T value;

        public AsyncGate() : base(false) { }
        /// <param name="open">Set the initial state of the gate to open or closed</param>
        public AsyncGate(bool open) : base(open: open) { }

        /// <summary>Open the gate, all awaits will resume immediately</summary>
        public void Open(T value) {
            this.value = value;
            Open();
        }

        /// <summary>Let all current awaiters through the gate. Does not affect open state.</summary>
        public void Release(T value) {
            this.value = value;
            Release();
        }

        /// <summary>Set the pass on value that is returned when the gate is awaited</summary>
        public void SetValue(T value) {
            this.value = value;
        }

        /// <summary>Wait for the gate to open</summary>
        public async Task<T> Wait() {
            await AwaitCompletion(waitIndefinitely, default);
            return value;
        }

        /// <summary>Wait for the gate to open</summary>
        public async Task<T> Wait(CancellationToken token) {
            await AwaitCompletion(waitIndefinitely, token);
            return value;
        }

        /// <summary>Wait for the gate to open</summary>
        public async Task<Job<T>> Wait(int timeout, CancellationToken token) {
            if (await AwaitCompletion(timeout, token)) {
                return Job.Completed(value);
            } else {
                return Job.Failed(value);
            }
        }

        /// <summary>Wait for the gate to open</summary>
        public async Task<Job<T>> Wait(int timeout) {
            if (await AwaitCompletion(timeout, default)) {
                return Job.Completed(value);
            } else {
                return Job.Failed(value);
            }
        }
    }
}
