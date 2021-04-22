using Apprentice.Hotkeys;
using Apprentice.Tools;
using Apprentice.Tools.Extensions;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.GUI {

    public static class GuiTool {

        #region user methods
        public static void Message(string text) => MessageBox.Create(text).Throw();
        public static void Message(string title, string text) => MessageBox.Create(title, text).Throw();
        public static void Tooltip(Image image, int duration = GUI.Tooltip.DefaultDuration, string identifier = null) => GUI.Tooltip.Create(image, duration, identifier).Throw();
        public static void Tooltip(string text, int duration = GUI.Tooltip.DefaultDuration, string identifier = null) => GUI.Tooltip.Create(text, duration, identifier).Throw();
        public static void Notification(object text) => NotificationBox.Popup(text?.ToString()).Throw();
        public static void Notification(object title, object text, Action lclick = null, Action rclick = null, EdgeType side = EdgeType.Left, Monitor monitor = null) => NotificationBox.Popup(title?.ToString(), text?.ToString(), lclick, rclick, side, monitor).Throw();
        public static void Warning(string title, string text, Action lclick = null, Action rclick = null, EdgeType side = EdgeType.Left, Monitor monitor = null) => NotificationBox.Warning(title, text, lclick, rclick, side, monitor).Throw();
        public static void Warning(string text) => NotificationBox.Warning("Warning", text).Throw();
        public static void Error(string title, string text, Action lclick = null, Action rclick = null, EdgeType side = EdgeType.Left, Monitor monitor = null) => NotificationBox.Error(title, text, lclick, rclick, side, monitor).Throw();
        public static void Error(string text) => NotificationBox.Error("Error", text).Throw();
        public static void Error(string title, Exception e) {
            Action action = () => Message(e.GetType().Name, e.ToString());
            NotificationBox.Error(title, $"A type {e.GetType().Name} error occurred", lclick: action).Throw();
        }
        public static Task<string> StringInput(string desc = null) => InputBox.Create(desc);
        public static Task<string> StringInput(string title, string desc, string input = null) => InputBox.Create(title, desc, input);
        public static Task<bool> Confirmation(string desc = null) => ConfirmBox.Create(desc);
        public static Task<bool> Confirmation(string title, string desc) => ConfirmBox.Create(title, desc);

        public static async Task Preview(Window window, int fps, Action<HAction> mButtonAction = null) {
            if (window == null || !window.Exists)
                return;

            Edge corner = new Edge(EdgeType.BottomRight, Monitor.Primary.Area.BottomRight - new Coord(60, 60));
            var area = new Area(0, 0, 576, 324).SetEdge(corner);

            var gui = new ImageBox(window.GetImage(), area);
            gui.Launch(form => form.ShowInTaskbar = false);
            await gui.WaitForForm();
            gui.Window.SetAlwaysOnTop(true);
            //try { gui.Window.Pin(true); } catch { }

            var t = new TaskCompletionSource<object>();

            bool Context() => Window.FromMouse == gui.Window;
            var focus = Hotkey.Create(Key.LButton, HAction.Single(a => window.Activate()), priority: 100, context: Context);
            var hotkey = Hotkey.Create(Key.RButton, HAction.Single(a => t.TrySetResult(null)), priority: 100, context: Context);
            var mba = Hotkey.Create(Key.MButton, HAction.Single(mButtonAction), priority: 100, context: Context);

            bool hidden = false;
            bool trans = false;

            while (!t.Task.IsCompleted) {
                await Task.Delay(1000 / fps);

                if (!window.Exists) {
                    break;

                } else if (window.IsActive) {
                    if (!hidden) {
                        gui.Window.SetOpacity(0);
                        gui.Window.SetClickThrough(true);
                        hidden = true;
                    }

                } else {
                    if (hidden) {
                        gui.Window.SetOpacity(100);
                        gui.Window.SetClickThrough(false);
                        hidden = false;
                    }

                    gui.SetImage(window.GetImagePrint(true));

                    var newArea = gui.Window.Area;
                    var contains = newArea.Contains(Mouse.Position);

                    if (contains && !trans) {
                        trans = true;
                        gui.Window.SetOpacity(50);
                    } else if (!contains && trans) {
                        trans = false;
                        gui.Window.SetOpacity(100);
                    }

                    if (newArea != area) {
                        newArea.SetEdge(corner);
                        area = newArea;
                        gui.Window.Move(area);
                    }
                }
            }

            gui.Close();
            hotkey.Remove();
            mba.Remove();
            focus.Remove();
            Console.WriteLine("Preview closed");
        }
        #endregion

        #region execution
        public static void Execute(Form form, Action action) {
            if (form.InvokeRequired) {
                try {
                    form.Invoke(action);
                } catch (InvalidAsynchronousStateException e) {
                    Error("Form Invoke Error", e);
                }
            } else {
                action.Invoke();
            }
        }

        public static T Execute<T>(Form form, Func<T> func) {
            if (form.InvokeRequired) {
                try {
                    return (T) form.Invoke(func);
                } catch (InvalidAsynchronousStateException e) {
                    Error("Form Invoke Error", e);
                    return default;
                }
            } else {
                return func.Invoke();
            }
        }

        public static async Task Execute(Form form, Func<Task> asyncAction) {
            if (form.InvokeRequired) {
                try {
                    await (Task) form.Invoke(asyncAction);
                } catch (InvalidAsynchronousStateException e) {
                    Error("Form Invoke Error", e);
                }
            } else {
                await asyncAction.Invoke();
            }
        }

        public static async Task<T> Execute<T>(Form form, Func<Task<T>> asyncAction) {
            if (form.InvokeRequired) {
                try {
                    return await (Task<T>) form.Invoke(asyncAction);
                } catch (InvalidAsynchronousStateException e) {
                    Error("Form Invoke Error", e);
                    return default;
                }
            } else {
                return await asyncAction.Invoke();
            }
        }
        #endregion

        #region GUI helper methods
        public static Size CalculateTextSize(string text, Font font, int width = 0) {
            if (width == 0)
                return TextRenderer.MeasureText(text, font, new Size(0, 0), TextFormatFlags.NoPadding);
            else
                return TextRenderer.MeasureText(text, font, new Size(width, 0), TextFormatFlags.NoPadding | TextFormatFlags.WordBreak);
        }
        #endregion
    }
}

namespace Apprentice.GUI.Extensions {
    public static class FormExtensions {

        public static void AddShadow(IntPtr hwnd) {
            WinAPI.DwmIsCompositionEnabled(out bool enabled);

            if (!enabled) {
                WinAPI.SetClassLong(hwnd, WinAPI.ClassLongFlags.GCL_STYLE, WinAPI.GetClassLong(hwnd, WinAPI.ClassLongFlags.GCL_STYLE) | (uint) WinAPI.ClassStyles.DropShadow);
            } else {
                int val = 2;
                WinAPI.MARGINS margins = new WinAPI.MARGINS(1, 1, 1, 1);

                WinAPI.DwmSetWindowAttribute(hwnd, WinAPI.DWMWINDOWATTRIBUTE.NCRenderingPolicy, ref val, Marshal.SizeOf(val));
                WinAPI.DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }
        }

        #region container
        public static void AddShadow(this GuiContainer gui) => AddShadow(gui.Window.Hwnd);
        #endregion

        #region forms
        public static void Execute(this Form form, Action action) => GuiTool.Execute(form, action);
        public static object Execute(this Form form, Func<object> action) => GuiTool.Execute(form, action);
        public static Task Execute(this Form form, Func<Task> asyncAction) => GuiTool.Execute(form, asyncAction);
        public static Task<object> Execute(this Form form, Func<Task<object>> asyncAction) => GuiTool.Execute(form, asyncAction);

        /// <summary>Transform size from the form's client area to the form's full area coordinates</summary>
        public static Size ClientToFull(this Form form, Size size) {
            return size + (form.Size - form.ClientSize);
        }

        /// <summary>Transform size from the form's full area to client area coordinates</summary>
        public static Size FullToClient(this Form form, Size size) {
            return size + (form.ClientSize - form.Size);
        }

        public static void AddShadow(this Form form) => AddShadow(form.Handle);
        #endregion

        #region RichTextBox
        private static int scrollAmount = 15;

        public static void Scroll(this RichTextBox control, int delta, bool allowOvershoot = true) {
            delta = (delta > 0 ? -scrollAmount : scrollAmount) * 4;
            Point point = new Point();
            WinAPI.SendMessage(control.Handle, WM.USER + 221, IntPtr.Zero, ref point);
            point.Y = allowOvershoot ? point.Y + delta : Math.Max(0, Math.Min(point.Y + delta, control.GetScrollLength() - control.ClientSize.Height + scrollAmount));
            WinAPI.SendMessage(control.Handle, WM.USER + 222, IntPtr.Zero, ref point);
        }

        public static void SetScrollPos(this RichTextBox control, int value) {
            Point point = new Point(0, value);
            WinAPI.SendMessage(control.Handle, WM.USER + 222, IntPtr.Zero, ref point);
        }

        public static int GetScrollPos(this RichTextBox control) {
            Point point = new Point();
            WinAPI.SendMessage(control.Handle, WM.USER + 221, IntPtr.Zero, ref point);
            return point.Y;
        }

        public static int GetScrollLength(this RichTextBox control) {
            int charstart = control.GetPositionFromCharIndex(0).Y;
            int charend = control.GetPositionFromCharIndex(control.TextLength - 1).Y;
            return charend - charstart;
        }

        public static void ScrollToBottom(this RichTextBox control) {
            Point point = new Point(0, 0);
            point.Y = Math.Max(0, control.GetScrollLength() - control.ClientSize.Height + scrollAmount);
            WinAPI.SendMessage(control.Handle, WM.USER + 222, IntPtr.Zero, ref point);
        }
        #endregion
    }
}
