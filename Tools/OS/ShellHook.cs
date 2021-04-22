using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.Tools {

    public static class ShellHook {

        private static ShellMonitor monitor;
        private static int message;

        /// <summary>Check if the shell hook has been installed</summary>
        public static bool Installed => monitor != null;

        public static event Action<Window> WindowActivated;
        public static event Action<Window> WindowCreated;
        public static event Action<Window> WindowDestroyed;
        public static event Action<Window> WindowFlashed;
        public static event Action<Window> WindowRedraw;

        /// <summary>Subscribe to shell messages</summary>
        /// <remarks>Must be registered in a thread that doesn't terminate and has a message loop</remarks>
        public static void RegisterShellHook() {
            monitor = new ShellMonitor();
            message = WinAPI.RegisterWindowMessage("SHELLHOOK");
            WinAPI.RegisterShellHookWindow(monitor.Handle);
        }

        private class ShellMonitor : Form {
            protected override void WndProc(ref Message m) {
                if (m.Msg == message) {
                    var type = (WinAPI.ShellHook) m.WParam;
                    switch (type) {
                        case WinAPI.ShellHook.Activate:
                        case WinAPI.ShellHook.RudeActivate:
                            WindowActivated?.Invoke(GetWindow(m.LParam));
                            break;
                        case WinAPI.ShellHook.Create:
                            WindowCreated?.Invoke(GetWindow(m.LParam));
                            break;
                        case WinAPI.ShellHook.Destroy:
                            WindowDestroyed?.Invoke(GetWindow(m.LParam));
                            break;
                        case WinAPI.ShellHook.Flash:
                            WindowFlashed?.Invoke(GetWindow(m.LParam));
                            break;
                        case WinAPI.ShellHook.Redraw:
                            WindowRedraw?.Invoke(GetWindow(m.LParam));
                            break;
                    }
                }

                base.WndProc(ref m);
            }

            private Window GetWindow(IntPtr hwnd) {
                if (Window.CachedWindows.ContainsKey(hwnd))
                    return Window.CachedWindows[hwnd];
                return new Window(hwnd);
            }
        }
    }

    public static class WinAwaiter {
        public static async Task<Window> WaitCreate(Func<Window, bool> predicate, int? timeout = null, bool ignoreCurrent = true) {
            if (predicate == null)
                throw new ArgumentNullException("Predicate can't be null");
            if (!ShellHook.Installed)
                throw new Exception("Shell hook has not been installed");
            if (!ignoreCurrent) {
                var win = Window.Find(predicate);
                if (win.IsValid) {
                    return win;
                }
            }

            var source = new TaskCompletionSource<Window>(TaskCreationOptions.RunContinuationsAsynchronously);
            void Action(Window w) {
                if (predicate(w)) {
                    ShellHook.WindowCreated -= Action;
                    source.SetResult(w);
                }
            }

            ShellHook.WindowCreated += Action;

            Window window;
            if (timeout is int tm) {
                var task = await Task.WhenAny(source.Task, Task.Delay(tm));
                if (task == source.Task) {
                    window = source.Task.Result;
                } else {
                    window = Window.None;
                    ShellHook.WindowCreated -= Action;
                }

            } else {
                window = await source.Task;
            }

            return window;
        }

        public static async Task<Window> WaitActive(Func<Window, bool> predicate, int? timeout = null, bool ignoreCurrent = true) {
            if (predicate == null)
                throw new ArgumentNullException("Predicate can't be null");
            if (!ShellHook.Installed)
                throw new Exception("Shell hook has not been installed");
            if (!ignoreCurrent) {
                var win = Window.Active;
                if (predicate(win)) {
                    return win;
                }
            }

            var source = new TaskCompletionSource<Window>(TaskCreationOptions.RunContinuationsAsynchronously);
            void Action(Window w) {
                if (predicate(w)) {
                    ShellHook.WindowActivated -= Action;
                    source.SetResult(w);
                }
            }

            ShellHook.WindowActivated += Action;

            Window window;
            if (timeout is int tm) {
                var task = await Task.WhenAny(source.Task, Task.Delay(tm));
                if (task == source.Task) {
                    window = source.Task.Result;
                } else {
                    window = Window.None;
                    ShellHook.WindowActivated -= Action;
                }

            } else {
                window = await source.Task;
            }

            return window;
        }

        public static async Task<Window> WaitClose(Func<Window, bool> predicate, int? timeout = null) {
            if (predicate == null)
                throw new ArgumentNullException("Predicate can't be null");
            if (!ShellHook.Installed)
                throw new Exception("Shell hook has not been installed");

            var source = new TaskCompletionSource<Window>(TaskCreationOptions.RunContinuationsAsynchronously);
            void Action(Window w) {
                if (predicate(w)) {
                    ShellHook.WindowDestroyed -= Action;
                    source.SetResult(w);
                }
            }

            ShellHook.WindowDestroyed += Action;

            Window window;
            if (timeout is int tm) {
                var task = await Task.WhenAny(source.Task, Task.Delay(tm));
                if (task == source.Task) {
                    window = source.Task.Result;
                } else {
                    window = Window.None;
                    ShellHook.WindowDestroyed -= Action;
                }

            } else {
                window = await source.Task;
            }

            return window;
        }
    }
}
