using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprentice.Tools {
    public static class WinSettings {
        // Full list of setting page commands
        // https://winaero.com/ms-settings-commands-in-windows-10/

        /// <summary>Open Windows audio device settings</summary>
        public static void OpenAppVolume() => Launcher.Start("ms-settings:apps-volume");
        /// <summary>Open Windows settings</summary>
        public static void OpenSettings() => Launcher.Start("ms-settings");
    }
}
