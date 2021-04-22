using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Tools {
    public static class AnimationManager {

        public static Dictionary<object, IAnimation> Animations { get; private set; } = new Dictionary<object, IAnimation>();
        private static readonly object locker = new object();

        public static List<IAnimation> WindowAnimations => Animations.Where(pair => pair.Key is Window).Select(pair => pair.Value).ToList();

        public static void Add(object identity, IAnimation animation) {
            lock (locker) {
                if (animation.ID == 0)
                    throw new Exception("Illegal animation ID");
                if (Animations.ContainsKey(identity)) {
                    if (Animations[identity].ID != animation.ID)
                        Animations[identity].Stop();
                    Animations[identity] = animation;
                } else {
                    Animations.Add(identity, animation);
                }
            }
        }

        public static void Remove(object identity, IAnimation animation) {
            lock (locker) {
                if (Animations.ContainsKey(identity) && Animations[identity].ID == animation.ID) {
                    Animations[identity].Stop();
                    Animations.Remove(identity);
                }
            }
        }

        public static void Remove(object identity) {
            lock (locker) {
                if (Animations.ContainsKey(identity)) {
                    Animations[identity].Stop();
                    Animations.Remove(identity);
                }
            }
        }

        public static IAnimation Fetch(object identity) {
            lock (locker) {
                if (Animations.ContainsKey(identity))
                    return Animations[identity];
                return null;
            }
        }
    }
}
