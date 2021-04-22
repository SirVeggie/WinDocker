using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Tools.Extensions {
    public static class WindowExtensions {
        public static void SetVolume(this Window window, float level) => window.Audio?.SetVolume(level);
        public static void SetMute(this Window window, bool state) => window.Audio?.SetMute(state);

        public static void AdjustVolume(this Window window, float percentage) {
            window.Audio.AdjustVolume(percentage);
        }

        public static WinAnimation Animate(this Window window, Area target, int duration, Curve curve) {
            return WinAnimation.Start(window, target, duration, curve);
        }

        public static WinAnimation Animate(this Window window, Area start, Area end, int duration, Curve curve) {
            return WinAnimation.Start(window, start, end, duration, curve);
        }

        public static bool HasAnimation(this Window window) => AnimationManager.Fetch(window) != null;

        public static IAnimation GetAnimation(this Window window) => AnimationManager.Fetch(window);

        public static void StopAnimation(this Window window) {
            AnimationManager.Remove(window);
        }

        public static Task WaitAnimation(this Window window) => AnimationManager.Fetch(window)?.WaitForStop() ?? Task.CompletedTask;
    }
}
