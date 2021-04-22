using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprentice.Tools {
    public class ActionQueue {

        private Queue<GeneralAction> queue = new Queue<GeneralAction>();
        public bool Running { get; private set; }
        private bool queueRunning;
        private TaskCompletionSource<object> source;

        public void Start() {
            if (Running)
                return;
            Running = true;
            _ = ResolveQueue();
        }

        public async Task StartAsync() {
            if (Running)
                await Wait();
            Running = true;
            await ResolveQueue();
        }

        public void Start(Action action) {
            Add(action);
            Start();
        }

        public void Start(Func<Task> action) {
            Add(action);
            Start();
        }

        public void Add(Action action) {
            queue.Enqueue(new GeneralAction(action));
        }

        public void Add(Func<Task> action) {
            queue.Enqueue(new GeneralAction(action));
        }

        public void Stop() {
            Running = false;
        }

        private async Task ResolveQueue() {
            if (queueRunning)
                return;
            queueRunning = true;
            source = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            while (Running && queue.Count > 0) {
                await queue.Dequeue().Run();
            }

            Running = false;
            queueRunning = false;
            source.SetResult(null);
        }

        public async Task Wait() {
            if (source == null)
                return;
            await source.Task;
        }

        public void Reset() {
            queue = new Queue<GeneralAction>();
        }
    }

    public class GeneralAction {
        public bool Async { get; }
        private Func<Task> asyncAction;
        private Action normalAction;

        public GeneralAction(Action action) {
            Async = false;
            if (action == null)
                throw new ArgumentNullException("Action can not be null");
            normalAction = action;
        }

        public GeneralAction(Func<Task> action) {
            Async = true;
            if (action == null)
                throw new ArgumentNullException("Action can not be null");
            asyncAction = action;
        }

        public async Task Run() {
            if (Async)
                await asyncAction();
            else
                normalAction();
        }
    }
}
