using Apprentice.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Hotkeys {
    public abstract class HotkeyProfile {

        public bool Initialized;
        public abstract bool Debug { get; }
        public abstract void Create();
        private void BaseCreate() {
            if (Initialized)
                throw new InvalidOperationException("This profile has already been created");
            Initialized = true;
            Create();
        }

        public static List<Type> GetProfiles() {
            return typeof(HotkeyProfile).GetSubclasses();
        }

        public static void CreateAll(bool debug) {
            var profiles = GetProfiles();

            foreach (var prof in profiles) {
                HotkeyProfile p = (HotkeyProfile) Activator.CreateInstance(prof);

                // Continue to next iteration if profile and global debug state do not match
                if (p.Debug != debug) {
                    continue;
                }

                var name = prof.Name.CamelCaseToWords();
                p.BaseCreate();
                Console.WriteLine("Loaded " + name);
            }
        }

        #region creation helpers
        protected static Hotkey Hotkey(TriggerKey keys, HAction action = null, Func<bool> context = null, HAction release = null, int priority = 0, bool block = true, bool wild = true, bool parallel = false) {
            return Hotkeys.Hotkey.Create(keys, action, context, release, priority, block, wild, parallel);
        }

        protected static Hotkey Hotkey(TriggerKey keys, Action<SingleAction> action = null, Func<bool> context = null, Action<SingleAction> release = null, int priority = 0, bool block = true, bool wild = true, bool parallel = false) {
            return Hotkeys.Hotkey.Create(keys, action == null ? null : HAction.Single(action), context, release == null ? null : HAction.Single(release), priority, block, wild, parallel);
        }

        protected static Hotkey Hotkey(TriggerKey keys, Func<SingleAction, Task> action = null, Func<bool> context = null, Func<SingleAction, Task> release = null, int priority = 0, bool block = true, bool wild = true, bool parallel = false) {
            return Hotkeys.Hotkey.Create(keys, action == null ? null : HAction.Single(action), context, release == null ? null : HAction.Single(release), priority, block, wild, parallel);
        }

        private static TriggerKey PresetModifier(Key main, Key[] prevList, params Key[] modifiers) => new TriggerKey(main, prevList.Concat(modifiers).ToArray());
        public static TriggerKey Win(Key main, params Key[] modifiers) => PresetModifier(main, modifiers, Key.Win);
        public static TriggerKey Shift(Key main, params Key[] modifiers) => PresetModifier(main, modifiers, Key.Shift);
        public static TriggerKey Ctrl(Key main, params Key[] modifiers) => PresetModifier(main, modifiers, Key.Ctrl);
        public static TriggerKey Alt(Key main, params Key[] modifiers) => PresetModifier(main, modifiers, Key.Alt);
        #endregion
    }
}
