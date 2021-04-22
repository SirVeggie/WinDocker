using Apprentice.GUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Debugging {
    public static class AreaDebug {
        public static BasicBox Show(Area area) {
            if (!area.IsValid)
                throw new ArgumentException("Area contained NaN values");
            var form = new BasicBox(area, Color.Red);

            form.OnShow += () => {
                form.Window.SetOpacity(0.3);
                form.Window.SetAlwaysOnTop(true);
            };

            form.Launch();
            return form;
        }

        public static void Show(Area area, int duration) => _ = ShowWait(area, duration);
        public static async Task ShowWait(Area area, int duration) {
            var box = Show(area);
            await Task.Delay(duration).ConfigureAwait(false);
            box.Close();
        }
    }
}
