using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Hotkeys {
    public static class HotkeyManager {

        public static Dictionary<Key, HotkeyGroup> Groups { get; set; } = new Dictionary<Key, HotkeyGroup>();
        public static Dictionary<Key, Hotkey> CurrentHotkeys { get; set; } = new Dictionary<Key, Hotkey>();
        public static Action<Hotkey, Exception> ExceptionHandler { get; set; }

        public static bool KeyDown(Key key) {
            var hotkey = FindHotkey(key);

            if (hotkey == null)
                return false;
            if (!key.IsStateless())
                SetCurrent(key, hotkey);
            Task.Run(hotkey.KeyDown);

            return hotkey.Block;
        }

        public static bool KeyUp(Key key) {
            if (!CurrentHotkeys.ContainsKey(key)) {
                return false;
            }

            var hotkey = CurrentHotkeys[key];
            CurrentHotkeys.Remove(key);
            Task.Run(hotkey.KeyUp);

            return hotkey.Block;
        }

        #region hotkey handling
        public static Hotkey FindHotkey(Key key) {
            if (!Groups.ContainsKey(key)) {
                return null;
            } else {
                return Groups[key].GetActive();
            }
        }

        public static bool AddHotkey(Hotkey hotkey) {
            if (!Groups.ContainsKey(hotkey.MainKey)) {
                Groups.Add(hotkey.MainKey, new HotkeyGroup(hotkey.MainKey));
            }

            return Groups[hotkey.MainKey].Add(hotkey);
        }

        public static bool RemoveHotkey(Hotkey hotkey) {
            if (!Groups.ContainsKey(hotkey.MainKey)) {
                throw new Exception("That hotkey doesn't exist");
            }

            return Groups[hotkey.MainKey].Remove(hotkey);
        }
        #endregion

        public static bool IsBlocked(Key key) {
            if (CurrentHotkeys.ContainsKey(key)) {
                return CurrentHotkeys[key].Block;
            } else {
                return false;
            }
        }

        private static void SetCurrent(Key key, Hotkey hotkey) {
            if (CurrentHotkeys.ContainsKey(key)) {
                throw new Exception($"An active hotkey of that type '{key}' already exists. Prev priority: {CurrentHotkeys[key].Priority}, new priority: {hotkey.Priority}");
            }

            CurrentHotkeys.Add(key, hotkey);
        }

        public static uint MapPriority(int priority, bool hasModifiers) {
            priority *= -2;
            if (hasModifiers)
                priority -= 1;

            if (priority < 0) {
                priority = priority + int.MaxValue + 1;
                return (uint) priority;
            } else {
                uint p = (uint) priority;
                p = p + int.MaxValue + 1;
                return p;
            }
        }

        public static int MapPriority(uint priority) {
            int i;
            if (priority >= (uint) int.MaxValue + 1) {
                i = (int) (priority - int.MaxValue - 1);
            } else {
                i = (int) priority;
                i = i - int.MaxValue - 1;
            }

            if (i % 2.0 == 1)
                i += 1;
            i /= -2;

            return i;
        }
    }
}
