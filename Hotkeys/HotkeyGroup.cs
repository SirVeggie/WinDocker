using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WinUtilities;

namespace Apprentice.Hotkeys {
    public class HotkeyGroup : IEnumerable<Hotkey> {

        public Key Key { get; }
        public SortedList<uint, Stack<Hotkey>> PriorityList { get; set; }

        public HotkeyGroup(Key key) {
            Key = key;
            PriorityList = new SortedList<uint, Stack<Hotkey>>();
        }

        public Hotkey GetActive() {
            foreach (Hotkey hotkey in this) {
                if (hotkey.IsActive) {
                    return hotkey;
                }
            }

            return null;
        }

        public bool Add(Hotkey hotkey) {
            var priority = HotkeyManager.MapPriority(hotkey.Priority, hotkey.Modifiers.Count > 0);

            if (!PriorityList.ContainsKey(priority))
                PriorityList.Add(priority, new Stack<Hotkey>());

            if (!PriorityList[priority].Contains(hotkey)) {
                PriorityList[priority].Push(hotkey);
                return true;
            } else {
                return false;
            }
        }

        public bool Remove(Hotkey hotkey) {
            var priority = HotkeyManager.MapPriority(hotkey.Priority, hotkey.Modifiers.Count > 0);

            if (!PriorityList.ContainsKey(priority)) {
                Console.WriteLine($"Unable to remove hotkey: Hotkey {hotkey.MainKey} not found");
                throw new Exception("Hotkey to remove not found");
            }

            var count = PriorityList[priority].Count;
            PriorityList[priority] = new Stack<Hotkey>(PriorityList[priority].Where(h => h.ID != hotkey.ID).Reverse().ToArray());

            return PriorityList[priority].Count < count;
        }

        public IEnumerator<Hotkey> GetEnumerator() {
            foreach (var pair in PriorityList) {
                foreach (Hotkey hotkey in pair.Value) {
                    yield return hotkey;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
