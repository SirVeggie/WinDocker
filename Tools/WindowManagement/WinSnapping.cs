using System;
using System.Collections.Generic;
using System.Linq;
using WinUtilities;

namespace Apprentice.Tools {
    public static class WinSnapping {

        public struct EdgeInfo {
            public int pos;
            public int min;
            public int max;

            public EdgeInfo(int pos, int min, int max) {
                this.pos = pos;
                this.min = min;
                this.max = max;
            }
        }

        /// <summary>Get valid snap edges</summary>
        /// <param name="match">Find edges of only matching windows</param>
        /// <param name="monitorCulling">Enable the culling of edges based on the location of the culling area and monitors</param>
        /// <param name="cullingArea">Current area to snap to use in culling</param>
        public static Dictionary<EdgeType, List<EdgeInfo>> GetEdges(IWinMatch match = null, bool monitorCulling = false, Area? cullingArea = null) {
            var windows = Window.GetWindows(match, WinFindMode.CurrentDesktop);

            var lefts = new List<EdgeInfo>();
            var rights = new List<EdgeInfo>();
            var tops = new List<EdgeInfo>();
            var bottoms = new List<EdgeInfo>();

            var areas = new List<Area>();

            if (monitorCulling) {
                if (cullingArea == null) {
                    throw new Exception("Snap area cannot be null when monitor culling");
                }

                var area = (Area) cullingArea;
                var mons = new List<Area>();

                foreach (var mon in Monitor.GetMonitors()) {
                    var monArea = mon.WorkArea;
                    if (area.Overlaps(monArea)) {
                        mons.Add(monArea);
                    }
                }

                foreach (var win in windows) {
                    var monArea = win.Area;
                    if (mons.Any(m => m.Overlaps(monArea))) {
                        areas.Add(monArea);
                    }
                }

                areas.AddRange(mons);

            } else {
                areas = windows.Select(w => w.Area).Concat(Monitor.GetMonitors().Select(m => m.WorkArea)).ToList();
            }

            foreach (var area in areas) {
                lefts.Add(new EdgeInfo((int) area.Left, (int) area.Top, (int) area.Bottom));
                rights.Add(new EdgeInfo((int) area.Right, (int) area.Top, (int) area.Bottom));
                tops.Add(new EdgeInfo((int) area.Top, (int) area.Left, (int) area.Right));
                bottoms.Add(new EdgeInfo((int) area.Bottom, (int) area.Left, (int) area.Right));
            }

            return new Dictionary<EdgeType, List<EdgeInfo>> {
                { EdgeType.Left, lefts },
                { EdgeType.Right, rights },
                { EdgeType.Top, tops },
                { EdgeType.Bottom, bottoms }
            };
        }

        public static Area FindSnapPosition(Area area, int maxDistance, bool resize = false) => FindSnapPosition(null, area.Round(), maxDistance, resize);
        public static Area FindSnapPosition(Window window, int maxDistance, bool resize = false) => FindSnapPosition(window.AsMatch.AsReverse, window.Area, maxDistance, resize);
        public static Area FindSnapPosition(IWinMatch match, Area area, int maxDistance, bool resize) {
            var edges = GetEdges(match, true, area);

            var vertical = edges[EdgeType.Left].Concat(edges[EdgeType.Right]).ToList();
            var horizontal = edges[EdgeType.Top].Concat(edges[EdgeType.Bottom]).ToList();

            int left = FindClosestMatch(new EdgeInfo((int) area.Left, (int) area.Top, (int) area.Bottom), vertical, maxDistance, out int dLeft);
            int right = FindClosestMatch(new EdgeInfo((int) area.Right, (int) area.Top, (int) area.Bottom), vertical, maxDistance, out int dRight);
            int top = FindClosestMatch(new EdgeInfo((int) area.Top, (int) area.Left, (int) area.Right), horizontal, maxDistance, out int dTop);
            int bottom = FindClosestMatch(new EdgeInfo((int) area.Bottom, (int) area.Left, (int) area.Right), horizontal, maxDistance, out int dBottom);


            if (resize) {
                if (dLeft <= maxDistance) area.LeftR = left;
                if (dRight <= maxDistance) area.RightR = right;
                if (dTop <= maxDistance) area.TopR = top;
                if (dBottom <= maxDistance) area.BottomR = bottom;
            } else {
                if (Math.Min(dLeft, dRight) <= maxDistance) {
                    if (dLeft <= dRight) {
                        area.Left = left;
                    } else {
                        area.Right = right;
                    }
                }

                if (Math.Min(dTop, dBottom) <= maxDistance) {
                    if (dTop <= dBottom) {
                        area.Top = top;
                    } else {
                        area.Bottom = bottom;
                    }
                }
            }

            return area;
        }

        private static int FindClosestMatch(EdgeInfo needle, List<EdgeInfo> edges, int dMax, out int distance) {
            var best = int.MinValue;
            distance = int.MaxValue;

            foreach (var edge in edges) {
                if (!FitsEdge(needle, edge, dMax))
                    continue;
                var d = Math.Abs(needle.pos - edge.pos);
                if (d < distance) {
                    best = edge.pos;
                    distance = d;
                }
            }

            return best;
        }

        private static bool FitsEdge(EdgeInfo needle, EdgeInfo edge, int dMax) => edge.min - dMax <= needle.max && needle.min <= edge.max + dMax;
    }
}
