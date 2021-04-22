using Apprentice.GUI.Extensions;
using Apprentice.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.Tools {
    public static class MouseHider {

        private static TaskCompletionSource<object> source;
        public static bool IsHidden { get; private set; }
        public static Thread Thread { get; private set; }

        public static void Hide() {
            if (IsHidden)
                return;
            source = new TaskCompletionSource<object>();
            Thread = new Thread(HideThread).RunSTA();
        }

        public static void Show() {
            source.TrySetResult(null);
            Console.WriteLine("source set");
        }

        private static void HideThread() {
            IsHidden = true;
            Console.WriteLine("Entering hide thread");
            Form form = new Form();
            new Window(form.Handle).SetOwner(Window.FromMouse);
            WinAPI.ShowCursor(false);
            source.Task.Wait();
            WinAPI.ShowCursor(true);
            form.Close();
            Console.WriteLine("Leaving hide thread");
            IsHidden = false;
        }
    }
}
