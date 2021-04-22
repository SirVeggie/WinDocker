using System;
using System.Runtime.InteropServices;
using WinUtilities;

namespace Apprentice.Tools {
    public static class AppConsole {

        public static Window Window => new Window(GetConsoleWindow());

        public static void Show(bool state) {
            if (state) Window.SetVisible(true);
            else Window.SetVisible(false);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
    }
}
