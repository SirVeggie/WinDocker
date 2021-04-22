//using Apprentice.Desktops;
using Apprentice.GUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Tools {

    public enum PermissionLevel {
        Current,
        Normal,
        Admin
    }

    public static class Launcher {

        public static bool RunningAsAdmin { get; } = WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);

        public static uint Start(string file, params string[] parameters) => Start(file, ProcessWindowStyle.Normal, null, PermissionLevel.Normal, parameters);
        public static uint Start(string file, PermissionLevel level, params string[] parameters) => Start(file, ProcessWindowStyle.Normal, null, level, parameters);
        public static uint Start(string file, ProcessWindowStyle style, string dir, PermissionLevel level, params string[] parameters) {
            return Base(file, string.Join(" ", parameters.Select(a => a.Contains(" ") ? $"\"{a}\"" : a)), dir, style, level);
        }

        public static uint Cmd(string command, PermissionLevel level = PermissionLevel.Normal) => Cmd(command, null, ProcessWindowStyle.Hidden, level);
        public static uint Cmd(string command, ProcessWindowStyle style, PermissionLevel level = PermissionLevel.Normal) => Cmd(command, null, style, level);
        public static uint Cmd(string command, string dir, PermissionLevel level = PermissionLevel.Normal) => Cmd(command, dir, ProcessWindowStyle.Hidden, level);
        public static uint Cmd(string command, string dir, ProcessWindowStyle style, PermissionLevel level) {
            return Base("cmd.exe", "/c " + command, dir, style, level);
        }

        #region base
        private static uint Base(string process, string arguments, string workingDir, ProcessWindowStyle style, PermissionLevel level) {
            if (process == null || process == "") {
                return 0;
            }

            if (level == PermissionLevel.Normal && RunningAsAdmin) {
                SystemUtility.ExecuteProcessUnElevated(process, arguments, workingDir ?? "");
                return 0;
            }

            Process p = new Process();
            p.StartInfo.FileName = process;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.WindowStyle = style;
            if (workingDir != null && workingDir != "")
                p.StartInfo.WorkingDirectory = workingDir;
            if (level == PermissionLevel.Admin)
                p.StartInfo.Verb = "RunAs";
            p.Start();

            try {
                return (uint) p.Id;
            } catch {
                return 0;
            }
        }
        #endregion
    }
}
