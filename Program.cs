using Apprentice.Hotkeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDocker {
    static class Program {

        public static event Action OnExit;

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppDomain.CurrentDomain.ProcessExit += (o, e) => OnExit();
            HotkeyProfile.CreateAll(false);
            KeyHandler.StartInput();

            Application.Run();
        }
    }
}
