using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtilities;
using Apprentice.Hotkeys;
using Apprentice.Personal.Tools;
using Apprentice.GUI;
using System.IO;

namespace WinDocker {
    public class DockHotkeys : HotkeyProfile {
        public override bool Debug => false;
        private static string keyfile = AppDomain.CurrentDomain.BaseDirectory + "dockkey.txt";
        private static Hotkey h;
        private static Key key;

        public override void Create() {
            LoadKey();

            Hotkey(Win(Key.Backspace), async a => {
                GuiTool.Message("WinDocker closing...");
                await Task.Delay(1000);
                Environment.Exit(0);
            });
            Hotkey(Win(Key.D0), async a => await Remap());

            CreateMainHotkey();
        }

        private static void CreateMainHotkey() {
            h = Hotkey(Win(key), a => {
                Window win = Window.Active;
                if (WindowDocker.IsEnabled(win)) {
                    WindowDocker.Disable(win);
                } else {
                    WindowDocker.Enable(win);
                }
            });
        }

        private static void LoadKey() {
            if (!File.Exists(keyfile)) {
                key = Key.Enter;
                return;
            }

            key = (Key) int.Parse(File.ReadAllText(keyfile));
        }

        private static async Task Remap() {
            var gui = new MessageBox("Window Docker", "Press any keyboard key to remap window docker to Win + [that key]");
            await gui.LaunchAndWait();
            gui.Window.SetAlwaysOnTop(true);
            gui.Window.MoveTop();

            key = (await KeyHandler.WaitKey(Filter)).Result.Key;
            gui.Close();
            File.WriteAllText(keyfile, ((int) key).ToString());

            h.Remove();
            CreateMainHotkey();

            GuiTool.Message("Remap succesful");

            bool Filter(Key k, bool state) {
                if (!state)
                    return false;
                if (k.IsKeyboard())
                    return true;
                return false;
            }
        }
    }
}
