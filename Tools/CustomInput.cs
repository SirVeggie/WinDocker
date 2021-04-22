using Apprentice.Hotkeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Tools {
    public static class CustomInput {

        /// <summary>Send keys while ignoring modifiers</summary>
        public static void Send(params Key[] keys) {
            var mods = KeyHandler.DisableModifiers();
            Input.Send(keys);
            KeyHandler.EnableModifiers(mods);
        }

        /// <summary>Send keys with event while ignoring modifiers</summary>
        public static void SendEvent(params Key[] keys) {
            var mods = KeyHandler.DisableModifiers();
            Input.SendEvent(keys);
            KeyHandler.EnableModifiers(mods);
        }

        /// <summary>Send text while ignoring modifiers, only affects special [] input</summary>
        public static void Send(string text, SendMode mode = SendMode.Input) {
            var mods = KeyHandler.DisableModifiers();
            Input.Send(text, mode);
            KeyHandler.EnableModifiers(mods);
        }

        public static void Copy(SendMode mode = SendMode.Input) {
            var mods = KeyHandler.DisableModifiers();
            Input.Send("[^][c]", mode);
            KeyHandler.EnableModifiers(mods);
        }

        public static void Paste(SendMode mode = SendMode.Input) {
            var mods = KeyHandler.DisableModifiers();
            Input.Send("[^][v]", mode);
            KeyHandler.EnableModifiers(mods);
        }
    }
}
