using System.Collections.Generic;
using System.Linq;
using WinUtilities;

namespace Apprentice.Hotkeys {
    /// <summary>Describes a key combination of a hotkey.</summary>
    public class TriggerKey {
        public Key MainKey { get; set; }
        public List<Key> Modifiers { get; set; }

        /// <summary>Create a key combination for a hotkey.</summary>
        public TriggerKey(Key main, params Key[] modifiers) {
            MainKey = main;
            Modifiers = modifiers.ToList();
        }

        public static implicit operator TriggerKey(Key key) => new TriggerKey(key);
    }

    public static class TriggerKeyExtensions {

        /// <summary>Add modifiers to a Key to make a HKey object.</summary>
        public static TriggerKey With(this Key key, params Key[] modifiers) => new TriggerKey(key, modifiers);
        public static TriggerKey With(this TriggerKey trigger, params Key[] modifiers) {
            trigger.Modifiers.AddRange(modifiers);
            return trigger;
        }
    }
}
