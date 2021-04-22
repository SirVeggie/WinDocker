using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Tools {
    public class WinAnimation : Animation {

        public Window Window { get; }
        public Area StartArea { get; private set; }
        public Area EndArea { get; private set; }
        public Curve Curve { get; }

        public WinAnimation(Window window, Area target, int duration, Curve curve) : base(duration, null, null, window) {
            if (Window.IsNullOrNone(window))
                throw new ArgumentException("Given window was null or none");
            if (curve == null)
                throw new ArgumentNullException("Given curve was null");

            Window = window;
            StartArea = window.Area;
            EndArea = target;

            AnimationCallback = t => window.Move(StartArea.Lerp(target, curve.Invoke(t)));
            CancelCallback = () => window.Move(StartArea);
        }

        public override IAnimation Copy() => new WinAnimation(Window, EndArea, Duration, Curve);

        public static WinAnimation Start(Window window, Area target, int duration, Curve curve) {
            var animation = new WinAnimation(window, target, duration, curve);
            animation.Start();
            return animation;
        }

        public static WinAnimation Start(Window window, Area start, Area end, int duration, Curve curve) {
            var animation = new WinAnimation(window, end, duration, curve);
            animation.StartArea = start;
            animation.Start();
            return animation;
        }
    }
}
