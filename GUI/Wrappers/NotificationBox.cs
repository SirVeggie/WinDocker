using Apprentice.Tools;
using Apprentice.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.GUI {
    public class NotificationBox : GuiContainer<NotificationForm> {
        public string Title => Execute(() => Form.Title);
        public string Content => Execute(() => Form.Content);

        private readonly string title;
        private readonly string content;
        private readonly Action leftClick;
        private readonly Action rightClick;

        public NotificationBox(string title, string content, Action leftClick = null, Action rightClick = null, Action onClose = null) {
            this.title = title;
            this.content = content;
            this.leftClick = leftClick;
            this.rightClick = rightClick;
        }

        protected override NotificationForm InitializeForm() => new NotificationForm(title, content, leftClick, rightClick);
        public void SetTitle(string text) => Execute(() => Form.SetTitle(text));
        public void SetContent(string text) => Execute(() => Form.SetContent(text));

        // Static methods

        public static List<NotificationBox> Notifications { get; private set; } = new List<NotificationBox>();
        public static int Lifetime { get; set; } = 5000;
        private static readonly object locker = new object();

        public static async Task Warning(string title, string text, Action lclick = null, Action rclick = null, EdgeType side = EdgeType.Left, Monitor monitor = null, Action<NotificationForm> init = null) {
            Action<NotificationForm> temp = form => {
                form.SetColors(titleBack: Color.Orange);
            };

            await Popup(title, text, lclick, rclick, side, monitor, temp + init);
        }

        public static async Task Error(string title, string text, Action lclick = null, Action rclick = null, EdgeType side = EdgeType.Left, Monitor monitor = null, Action<NotificationForm> init = null) {
            Action<NotificationForm> temp = form => {
                form.SetColors(titleBack: Color.IndianRed);
            };

            await Popup(title, text, lclick, rclick, side, monitor, temp + init);
        }

        public static async Task Popup(string text) => await Popup(Process.GetCurrentProcess().ProcessName, text);
        public static async Task Popup(string title, string text, Action lclick = null, Action rclick = null, EdgeType side = EdgeType.Left, Monitor monitor = null, Action<NotificationForm> init = null) {
            if (monitor == null)
                monitor = Monitor.Primary;
            side = side == EdgeType.Right ? EdgeType.Right : EdgeType.Left;

            var gui = new NotificationBox(title, text, lclick, rclick);
            gui.Launch(init);
            await gui.WaitForForm();

            gui.Window.SetAlwaysOnTop(true);
            gui.Window.MoveTop();
            gui.Window.Move(gui.Window.Area.SetEdge(EdgeType.Top | side, monitor.Area.GetEdge(EdgeType.Bottom | side).Pos));
            gui.Window.Animate(gui.Window.Area.SetEdge(monitor.WorkArea.GetEdge(EdgeType.Bottom | side)), 500, Curves.FExpo);

            lock (locker) {
                foreach (var prev in Notifications) {
                    if (!prev.IsAlive)
                        continue;
                    var animation = AnimationManager.Fetch(prev.Window) as WinAnimation;
                    var area = animation == null ? prev.Window.Area : animation.EndArea;
                    prev.Window.Animate(area.AddPoint(gui.Window.Area.W, 0), 500, Curves.FExpo);
                }
            }

            lock (locker) { Notifications.Add(gui); }
            await Task.Delay(Lifetime);
            lock (locker) { Notifications.Remove(gui); }

            if (gui.IsAlive) {
                var area = gui.Window.Area;
                await gui.Window.Animate(area.AddPoint(0, area.H), 500, Curves.FExpo).WaitForStop();
            }

            gui.Close();
        }
    }
}
