using Apprentice.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprentice.Tools.Extensions {
    public static class TaskExtensions {

        /// <summary>Causes an immediate notification if an exception occurs within the task</summary>
        public static void Throw(this Task task) {
            task.ContinueWith(t => {
                GuiTool.Message("Unhandled Task Exception", t.Exception.Flatten().ToString());
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
