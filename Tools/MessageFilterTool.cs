using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Apprentice.Tools {
    public static class MessageFilterTool {

        public static IMessageFilter Create(Func<Message, bool> predicate, Func<Message, bool> action) => new Filter(predicate, action);
        public static IMessageFilter Create(Func<Message, bool> predicate, Action<Message> action) => new Filter(predicate, action);
        public static IMessageFilter Create(Func<Message, bool> predicate, Func<Message, Task> asyncAction) => new AsyncFilter(predicate, asyncAction);

        private class Filter : IMessageFilter {

            private Func<Message, bool> predicate;
            private Func<Message, bool> action;

            public Filter(Func<Message, bool> predicate, Func<Message, bool> action) {
                this.predicate = predicate;
                this.action = action;
            }

            public Filter(Func<Message, bool> predicate, Action<Message> action) {
                this.predicate = predicate;
                this.action = m => { action(m); return false; };
            }

            public bool PreFilterMessage(ref Message m) {
                if (predicate(m))
                    return action(m);
                return false;
            }
        }

        private class AsyncFilter : IMessageFilter {

            private Func<Message, bool> predicate;
            private Func<Message, Task> action;

            public AsyncFilter(Func<Message, bool> predicate, Func<Message, Task> action) {
                this.predicate = predicate;
                this.action = action;
            }

            public bool PreFilterMessage(ref Message m) {
                if (predicate(m))
                    _ = action(m);
                return false;
            }
        }
    }
}
