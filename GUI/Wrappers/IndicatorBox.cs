using Apprentice.Tools;
using Apprentice.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.GUI {
    public class IndicatorBox : TextBox {

        public Func<Area, Area> AreaSetter { get; set; }
        public Func<string> ContentGetter { get; set; }
        public int UpdateDelay { get; set; } = 10;
        private Func<Area, Area> defaultSetter = area => area.SetEdge(Monitor.FromArea(area).WorkArea.TopLeftEdge);

        public IndicatorBox(string content, Func<Area, Area> areaSetter = null) : base(content, new Coord(0, -10000)) {
            AreaSetter = areaSetter ?? defaultSetter;
            Init();
        }

        public IndicatorBox(Func<string> getContent, Func<Area, Area> areaSetter = null) : base(getContent(), new Coord(0, -10000)) {
            if (getContent == null)
                throw new ArgumentNullException("Content getter can't be null");
            AreaSetter = areaSetter ?? defaultSetter;
            ContentGetter = getContent;
            Init();
        }

        private async void Init() {
            await WaitForForm();
            Window.SetClickThrough(true);
            Window.SetAlwaysOnTop(true);
            Window.MoveTop();
            try {
                //Window.Pin(true);
            } catch { Console.WriteLine($"{nameof(IndicatorBox)} pinning failed"); }

            var area = Window.Area;
            area.BottomLeft = Monitor.Primary.Area.TopLeft;
            Window.Move(area);
            await Window.Animate(AreaSetter(Window.Area), 500, Curves.FExpo).WaitForStop();

            var transparent = false;
            while (IsAlive) {
                var contains = Window.ContainsMouse();
                if (!transparent && contains) {
                    transparent = true;
                    Window.SetOpacity(0.2);
                } else if (transparent && !contains) {
                    transparent = false;
                    Window.SetOpacity(1);
                }

                if (ContentGetter != null)
                    SetContent(ContentGetter.Invoke());
                await Task.Delay(UpdateDelay);
            }
        }

        public override void Close() {
            var area = Window.Area;
            area.Y -= area.H;
            Window.Animate(area, 500, Curves.FExpo).WaitForStop().ContinueWith(t => base.Close());
        }
    }
}
