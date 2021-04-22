using Apprentice.GUI;
using Apprentice.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Tools {
    public class MonitorSplit {

        private static readonly object locker = new object();

        public static Dictionary<Window, MonitorSplit> Splits { get; set; } = new Dictionary<Window, MonitorSplit>();
        public static Action<Window, Area> MoveAction { get; set; } = (w, a) => w.Animate(a, 500, Curves.FExpo);


        private Coord pointer = new Coord(1, 1);
        private Coord parts = new Coord(1, 1);

        public Monitor Monitor { get; private set; }
        public Area PreviousArea { get; private set; }

        public Area Area => GetArea();
        public double CenterX => parts.X / 2 + 0.5;
        public double CenterY => parts.Y / 2 + 0.5;
        public bool IsCenterX => pointer.X == CenterX;
        public bool IsCenterY => pointer.Y == CenterY;
        public bool IsCenter => IsCenterX && IsCenterY;

        public MonitorSplit(Window window) {
            PreviousArea = window.HasAnimation() ? (window.GetAnimation() as WinAnimation)?.EndArea ?? window.Area : window.Area;
            Monitor = window.Monitor;
            WinAwaiter.WaitClose(w => w == window).ContinueWith(t => Remove(window));
        }

        #region static
        public static void SplitLeft(Window window) {
            lock (locker) {
                if (!Splits.ContainsKey(window))
                    Splits.Add(window, new MonitorSplit(window));
                window.Restore();
                var split = Splits[window];
                split.SplitLeft();
                if (split.parts.X == 1 && split.parts.Y == 1)
                    Cancel(window);
                else
                    MoveAction.Invoke(window, split.Area);
            }
        }

        public static void SplitRight(Window window) {
            lock (locker) {
                if (!Splits.ContainsKey(window))
                    Splits.Add(window, new MonitorSplit(window));
                window.Restore();
                var split = Splits[window];
                split.SplitRight();
                if (split.parts.X == 1 && split.parts.Y == 1)
                    Cancel(window);
                else
                    MoveAction.Invoke(window, split.Area);
            }
        }

        public static void SplitUp(Window window) {
            lock (locker) {
                if (!Splits.ContainsKey(window))
                    Splits.Add(window, new MonitorSplit(window));
                window.Restore();
                var split = Splits[window];
                split.SplitUp();
                if (split.parts.X == 1 && split.parts.Y == 1)
                    Cancel(window);
                else
                    MoveAction.Invoke(window, split.Area);
            }
        }

        public static void SplitDown(Window window) {
            lock (locker) {
                if (!Splits.ContainsKey(window))
                    Splits.Add(window, new MonitorSplit(window));
                window.Restore();
                var split = Splits[window];
                split.SplitDown();
                if (split.parts.X == 1 && split.parts.Y == 1)
                    Cancel(window);
                else
                    MoveAction.Invoke(window, split.Area);
            }
        }

        public static void MoveLeft(Window window) {
            lock (locker) {
                if (!Splits.ContainsKey(window))
                    return;
                window.Restore();
                var split = Splits[window];
                split.MoveLeft();
                MoveAction.Invoke(window, split.Area);
            }
        }

        public static void MoveRight(Window window) {
            lock (locker) {
                if (!Splits.ContainsKey(window))
                    return;
                window.Restore();
                var split = Splits[window];
                split.MoveRight();
                MoveAction.Invoke(window, split.Area);
            }
        }

        public static void MoveUp(Window window) {
            lock (locker) {
                if (!Splits.ContainsKey(window))
                    return;
                window.Restore();
                var split = Splits[window];
                split.MoveUp();
                MoveAction.Invoke(window, split.Area);
            }
        }

        public static void MoveDown(Window window) {
            lock (locker) {
                if (!Splits.ContainsKey(window))
                    return;
                window.Restore();
                var split = Splits[window];
                split.MoveDown();
                MoveAction.Invoke(window, split.Area);
            }
        }

        public static void Cancel(Window window) {
            lock (locker) {
                if (Splits.TryGetValue(window, out var split)) {
                    MoveAction.Invoke(window, split.PreviousArea);
                    Remove(window);
                }
            }
        }

        public static void Remove(Window window) {
            lock (locker) {
                Splits.Remove(window);
            }
        }
        #endregion

        #region instance
        private Area GetArea() {
            Area full = Monitor.WorkArea;
            int w = (int) full.W / parts.IntX + (IsCenterX ? (int) full.W % parts.IntX : 0);
            int h = (int) full.H / parts.IntY + (IsCenterY ? (int) full.H % parts.IntY : 0);
            int x = w * (pointer.IntX - 1) + (pointer.X >= CenterX ? (int) full.W % parts.IntX : 0) + Monitor.WorkArea.IntX;
            int y = h * (pointer.IntY - 1) + (pointer.Y >= CenterY ? (int) full.H % parts.IntY : 0) + Monitor.WorkArea.IntY;

            return new Area(x, y, w, h);
        }

        #region split
        public void SplitLeft() {
            if (pointer.X <= CenterX) {
                parts.X++;
            } else {
                parts.X--;
                pointer.X--;
            }
        }

        public void SplitRight() {
            if (pointer.X >= CenterX) {
                parts.X++;
                pointer.X++;
            } else {
                parts.X--;
            }
        }

        public void SplitUp() {
            if (pointer.Y <= CenterY) {
                parts.Y++;
            } else {
                parts.Y--;
                pointer.Y--;
            }
        }

        public void SplitDown() {
            if (pointer.Y >= CenterY) {
                parts.Y++;
                pointer.Y++;
            } else {
                parts.Y--;
            }
        }
        #endregion

        #region move
        public void MoveLeft() {
            if (pointer.X > 1) {
                pointer.X--;
            } else {
                pointer.X = parts.X;
                Monitor = Monitor.Previous();
            }
        }

        public void MoveRight() {
            if (pointer.X < parts.X) {
                pointer.X++;
            } else {
                pointer.X = 1;
                Monitor = Monitor.Next();
            }
        }

        public void MoveUp() {
            if (pointer.Y > 1) {
                pointer.Y--;
            }
        }

        public void MoveDown() {
            if (pointer.Y < parts.Y) {
                pointer.Y++;
            }
        }
        #endregion

        #endregion
    }
}
