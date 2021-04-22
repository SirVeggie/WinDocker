using Apprentice.GUI;
using Apprentice.Hotkeys;
using Apprentice.Tools;
using Apprentice.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDocker;
using WinUtilities;

namespace Apprentice.Personal.Tools {
    public class WindowDocker {

        public static Dictionary<Window, WindowDocker> HiddenWindows { get; private set; } = new Dictionary<Window, WindowDocker>();
        private static readonly int previewWidth = 10;
        private static readonly int peekWidth = 40;
        private static readonly int previewDelay = 1000;
        private static readonly int flickDelay = 250;
        private static readonly float previewOpacity = 0.4f;

        private static readonly bool notifications = false;

        public Window Window { get; }

        public bool IsAlwaysOnTop { get; private set; }
        public bool State { get; private set; }
        public Area ActiveArea { get; set; }
        public Area HiddenArea { get; set; }
        public Area PreviewArea { get; set; }
        public Area PeekArea { get; set; }

        private Window previous;
        private bool noCleanup;

        private WindowDocker(Window window) {
            Window = window;
        }

        private void ShowAnimate() {
            Area start = Window.Area;
            Animation.Start(750, identity: Window, action: t => {
                Window.Move(start.Lerp(ActiveArea, 0.5 + Curves.FExpo(t) / 2));
                t = Curves.Custom.FPower(10)(t);
                if (Window.Opacity < t)
                    Window.SetOpacity(t);
            });
        }

        private void HideAnimate() {
            Area start = Window.Area;
            Animation.Start(750, identity: Window, action: t => {
                Window.Move(start.Lerp(HiddenArea, Curves.FExpo(t)));
                t = 1 - Curves.Custom.FPower(10)(t);
                if (Window.Opacity > t)
                    Window.SetOpacity(t);
            });
        }

        private void PreviewShow() {
            if (!Window.IsAlwaysOnTop) {
                Window.Activate(WinActivateMode.Force);
                Window.SetAlwaysOnTop(true);
            }

            Window.Animate(HiddenArea, PeekArea, 300, Curves.FExpo);
            Window.SetOpacity(previewOpacity);
        }

        private async void PreviewHide() {
            await Window.Animate(HiddenArea, 300, Curves.FExpo).WaitForStop();
            Window.SetOpacity(0);
        }

        #region static
        public static bool IsEnabled(Window window) => HiddenWindows.ContainsKey(window);

        public static bool Disable(Window window, bool instant = false) {
            if (!HiddenWindows.ContainsKey(window))
                return false;
            if (instant)
                HiddenWindows[window].Deactivate();
            HiddenWindows.Remove(window);
            return true;
        }

        public static void Notification(string text) {
            if (!notifications)
                return;
            GuiTool.Notification("Window Docker", text);
        }

        public static void Enable(Window window) => Enable(window, EdgeType.None);
        public static void Enable(Window window, EdgeType direction) => EnableAsync(window, direction).Throw();
        public static async Task<bool> EnableAsync(Window window) => await EnableAsync(window, EdgeType.None);
        public static async Task<bool> EnableAsync(Window window, EdgeType direction) {
            if (direction.IsCorner())
                throw new ArgumentException($"{nameof(direction)} cannot be a corner");
            if (HiddenWindows.ContainsKey(window)) {
                Notification($"{window.Title}\nis already in a hide animation");
                return false;
            }

            if (direction.IsNone() is bool manual && manual) {
                direction = await GetInput();
                if (direction.IsNone()) return false;
            }

            window.Restore();
            var monitor = window.Monitor.Area;
            var hider = new WindowDocker(window);
            hider.IsAlwaysOnTop = window.IsAlwaysOnTop;
            hider.ActiveArea = hider.HiddenArea = window.Area;

            hider.ActiveArea = hider.ActiveArea.SetEdge(monitor.GetEdge(direction));
            hider.HiddenArea = hider.HiddenArea.SetEdge(direction.Reverse(), monitor.GetEdge(direction).Pos);
            hider.PreviewArea = hider.HiddenArea.SetEdge(direction, direction.IsTopOrLeft() ? previewWidth : -previewWidth, relative: true);
            hider.PeekArea = hider.HiddenArea.SetEdge(direction, direction.IsTopOrLeft() ? peekWidth : -peekWidth, relative: true);

            if (manual)
                Notification("Hide animation set succesfully");
            HiddenWindows.Add(window, hider);
            Program.OnExit += hider.Deactivate;
            window.SetAlwaysOnTop(true);
            hider.State = false;
            hider.Hide();
            hider.Loop();

            return true;
        }

        private static async Task<EdgeType> GetInput() {
            Notification("Press arrow key to set hide direction");
            var gui = new MessageBox("Window Docker", "Press WASD or arrow keys to set direction");
            await gui.LaunchAndWait();
            gui.Window.SetAlwaysOnTop(true);
            gui.Window.MoveTop();

            var key = await KeyHandler.WaitKeyDown(new Key[9] { Key.Left, Key.Right, Key.Up, Key.Down, Key.W, Key.A, Key.S, Key.D, Key.Escape }, block: true);
            var dir = GetDirection(key);

            gui.Close();
            if (dir.IsNone())
                Notification("Window hiding cancelled");
            return dir;
        }

        public static EdgeType GetDirection(Key key) {
            if (key == Key.Left) return EdgeType.Left;
            if (key == Key.Right) return EdgeType.Right;
            if (key == Key.Up) return EdgeType.Top;
            if (key == Key.Down) return EdgeType.Bottom;
            if (key == Key.A) return EdgeType.Left;
            if (key == Key.D) return EdgeType.Right;
            if (key == Key.W) return EdgeType.Top;
            if (key == Key.S) return EdgeType.Bottom;
            return 0;
        }
        #endregion

        private async void Loop() {
            while (HiddenWindows.ContainsKey(Window) && Window.Exists) {
                await Task.Delay(100);

                if (!State && PreviewArea.Contains(Mouse.Position) && Window.IsOnCurrentDesktop) {
                    if (Window.HasAnimation()) {
                        Window.StopAnimation();
                        Window.Move(HiddenArea);
                        Window.SetOpacity(0);
                        await Task.Yield();
                    }

                    PreviewShow();
                    bool flick = await WaitForFlick();
                    if (flick)
                        Show();
                    else
                        PreviewHide();
                    await WaitForLeave();
                } else if (State && (!ActiveArea.Contains(Mouse.Position) || !Window.IsOnCurrentDesktop)) {
                    Hide();
                }
            }

            Program.OnExit -= Deactivate;
            if (noCleanup)
                return;
            if (!Window.Exists)
                return;
            if (!IsAlwaysOnTop)
                Window.SetAlwaysOnTop(false);
            Show();
        }

        public async Task<bool> WaitForFlick() {
            var watch = Stopwatch.StartNew();

            while (watch.ElapsedMilliseconds < previewDelay) {
                await Task.Delay(10);
                if (!PeekArea.Contains(Mouse.Position)) {
                    return watch.ElapsedMilliseconds < flickDelay && ActiveArea.Contains(Mouse.Position);
                }
            }

            return false;
        }

        public async Task WaitForLeave() {
            while (PeekArea.Contains(Mouse.Position)) {
                await Task.Delay(100);
            }
        }

        public void Show() {
            State = true;
            previous = Window.Active;
            Window.MoveTop();
            ShowAnimate();
            Window.SetClickThrough(false);
        }

        public void Hide() {
            State = false;
            Window.Restore();
            HideAnimate();
            Window.SetClickThrough(true);

            if (Window.Active != Window) {
                return;
            } else if (previous == null || previous == Window || !previous.IsOnCurrentDesktop) {
                Window.Deactivate();
            } else {
                previous.Activate(WinActivateMode.Force);
            }
        }

        private void Deactivate() {
            noCleanup = true;
            if (!IsAlwaysOnTop)
                Window.SetAlwaysOnTop(false);
            Window.Move(ActiveArea);
            Window.SetOpacity(1);
            Window.SetClickThrough(false);
        }
    }
}
