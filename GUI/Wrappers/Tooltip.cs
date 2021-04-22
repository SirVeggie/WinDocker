using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.GUI {
    public class Tooltip {

        public bool IsImage { get; }
        public string Text { get; }
        public Image Image { get; }
        public Window Window { get; private set; }
        private GuiContainer Gui { get; }
        public TextBox AsTextBox => !IsImage ? (TextBox) Gui : throw new Exception("Tried to get Tooltip as Text when it is an Image");
        public ImageBox AsImageBox => IsImage ? (ImageBox) Gui : throw new Exception("Tried to get Tooltip as Image when it is a Text");

        private static Coord SpawnLocation => Mouse.Position + new Coord(20, 20);
        private static int ImageMaxWidth = (int) Monitor.Primary.Area.W / 2;

        public Tooltip(string text) {
            IsImage = false;
            Text = text;
            Gui = new TextBox(text, SpawnLocation);
            Setup();
        }

        public Tooltip(Image image) {
            IsImage = true;
            Image = image;
            Gui = new ImageBox(image, new Area(SpawnLocation, new Coord(ImageMaxWidth, 0)));
            Setup();
        }

        private async void Setup() {
            await Gui.LaunchAndWait();
            Gui.Window.SetAlwaysOnTop(true);
            Gui.Window.MoveTop();
        }

        public void Close() => Gui.Close();

        // Static methods

        public const int DefaultDuration = 750;
        public static Dictionary<string, TaskCompletionSource<object>> Tooltips { get; private set; } = new Dictionary<string, TaskCompletionSource<object>>();

        public static async Task Create(string text, int duration = DefaultDuration, string identifier = null) => await Create(new Tooltip(text), duration, identifier);
        public static async Task Create(Image image, int duration = DefaultDuration, string identifier = null) => await Create(new Tooltip(image), duration, identifier);
        private static async Task Create(Tooltip tip, int duration = DefaultDuration, string identifier = null) {
            if (identifier == null) {
                await Task.Delay(duration);
                tip.Close();
                return;
            }

            var source = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (Tooltips.ContainsKey(identifier)) {
                Tooltips[identifier].SetResult(null);
                Tooltips[identifier] = source;
                Console.WriteLine("Source replaced");
            } else {
                Tooltips.Add(identifier, source);
                Console.WriteLine("Source added");
            }

            await Task.WhenAny(Task.Delay(duration), source.Task);

            if (Tooltips[identifier] == source) {
                Tooltips.Remove(identifier);
                Console.WriteLine("Similar source removed");
            }

            tip.Close();
            Console.WriteLine("Tooltip closed");
        }

        public static bool Close(string identifier) {
            if (Tooltips.ContainsKey(identifier)) {
                Tooltips[identifier].SetResult(null);
                Tooltips.Remove(identifier);
                return true;
            }

            return false;
        }
    }
}
