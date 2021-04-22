using Apprentice.Debugging;
using Apprentice.GUI;
using Apprentice.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.Tools {
    public static class ClipboardHistory {

        /// <summary>History size</summary>
        public const int Size = 100;
        /// <summary>Time until preview is closed</summary>
        public const int Timeout = 1500;

        private static bool subscribed;
        private static int disableCounter;
        private static int pointer;
        private static long prevTime;
        private static Coord mouseOffset = new Coord(20, 20);

        private static long lastUpdate;
        private static long updateTimeout = 100;
        private static int enableDelay = 10;

        public static AsyncTimer Timer { get; private set; }
        public static ClipPreviewBox LastGui { get; private set; }

        public static DropStack<ClipboardData> History { get; private set; } = new DropStack<ClipboardData>(Size);
        public static bool IsEnabled => disableCounter == 0;

        public static void Start() {
            if (subscribed)
                return;
            subscribed = true;
            Clipboard.Updated += HandleUpdate;
        }

        private static void HandleUpdate(ClipboardData data) {
            if (data.IsEmpty) {
                return;
            }

            if (SameAsLast(data)) {
                return;
            }

            if (IsEnabled) {
                if (Time.Now <= lastUpdate + updateTimeout) {
                    History[0] = data;
                } else {
                    History.Push(data);
                    GuiTool.Tooltip("saved", 500);
                }

                lastUpdate = Time.Now;
            }
        }

        #region similarity
        public static bool SameAsLast(string text) => History.Count > 0 && text == History[0].GetText();

        public static bool SameAsLast(Image image) {
            if (History.Count == 0)
                return false;
            if (!History[0].HasImage)
                return false;
            if (image.Size == History[0].GetImage().Size)
                return true;
            return false;
        }

        public static bool SameAsLast(ClipboardData data) {
            if (History.Count == 0)
                return false;
            if (data.HasText && History[0].HasText && data.GetText() == History[0].GetText())
                return true;
            if (data.HasImage && SameAsLast(data.GetImage()))
                return true;
            return false;
        }
        #endregion

        #region ignore
        public static void Disable() {
            if (!subscribed)
                throw new Exception("Clipboard history is not turned on");
            disableCounter++;
            Clipboard.Updated -= HandleUpdate;
        }

        public static void Enable() {
            if (!subscribed)
                throw new Exception("Clipboard history is not turned on");
            if (disableCounter == 0)
                throw new Exception("Clipboard history recording is already enabled");
            disableCounter--;

            if (IsEnabled) {
                Clipboard.Updated += HandleUpdate;
            }
        }
        #endregion

        #region browsing
        public static void Left() {
            if (History.Count == 0)
                return;
            var now = Time.Now;
            if (now > prevTime + Timeout)
                pointer = 0;
            if (pointer > 0)
                pointer--;
            prevTime = now;
            SetClipboard(History[pointer].Data);
            Show();
        }

        public static void Right() {
            if (History.Count == 0)
                return;
            var now = Time.Now;
            if (now > prevTime + Timeout)
                pointer = 0;
            else if (pointer < 100 && pointer < History.Count - 1)
                pointer++;
            prevTime = now;
            SetClipboard(History[pointer].Data);
            Show();
        }

        public static void Show() {
            var clip = History[pointer];
            var desc = clip.HasImage ? $"{pointer + 1}:" : $"{pointer + 1}: {clip}";

            if (LastGui?.Grabbed ?? false)
                Timer.Stop();
            if (Timer?.Running ?? false) {
                Timer.Adjust(Timeout);
                if (clip.HasImage)
                    LastGui.SetImage(clip.GetImage());
                else
                    LastGui.SetImage(null);
                LastGui.SetDesc(desc);
                return;
            }

            ClipPreviewBox gui = null;

            if (clip.HasImage) {
                gui = new ClipPreviewBox(clip.GetImage(), desc, Mouse.Position + mouseOffset);
            } else {
                gui = new ClipPreviewBox(desc, Mouse.Position + mouseOffset);
            }

            gui.Launch();
            Timer = new AsyncTimer(() => ConditionalClose(gui));
            Timer.Start(Timeout);
            LastGui = gui;
            Loop();

            async void Loop() {
                await gui.WaitForForm();
                while (gui.IsAlive && !gui.Grabbed) {
                    gui.Window.Move(gui.Window.Area.Point.Lerp(Mouse.Position + mouseOffset, 0.1));
                    await Task.Delay(5);
                }
            }
        }

        private static void ConditionalClose(ClipPreviewBox gui) {
            if (!gui.Grabbed) {
                gui.Close();
            }
        }
        #endregion

        /// <summary>Set clipboard data without affecting clipboard history</summary>
        public static void SetClipboard(IDataObject data) {
            Disable();
            Task t = Clipboard.WaitForChange(1000);
            Clipboard.SetData(data);
            DelayedEnable(t);
        }

        /// <summary>Set clipboard text without affecting clipboard history</summary>
        public static void SetClipboard(string text) {
            Disable();
            Task t = Clipboard.WaitForChange(1000);
            Clipboard.SetText(text);
            DelayedEnable(t);
        }

        /// <summary>Set clipboard text without affecting clipboard history</summary>
        public static void SetClipboard(Image image) {
            Disable();
            Task t = Clipboard.WaitForChange(1000);
            Clipboard.SetImage(image);
            DelayedEnable(t);
        }

        /// <summary>Send Ctrl + C without affecting clipboard history</summary>
        public static async Task<ClipboardData> Copy() {
            Disable();
            var task = Clipboard.WaitForChange(5000);
            CustomInput.Copy();
            var res = await task;
            DelayedEnable(Task.CompletedTask);
            return res.Result;
        }

        /// <summary>Paste content and restore previous content afterwards</summary>
        public static async Task Paste(string text) {
            var data = Clipboard.GetData();
            Disable();
            Clipboard.SetText(text);
            await Clipboard.WaitForChange(1000);
            CustomInput.Paste();
            RestoreClip(data);
        }

        /// <summary>Paste content and restore previous content afterwards</summary>
        public static async Task Paste(Image image) {
            var data = Clipboard.GetData();
            Disable();
            Clipboard.SetImage(image);
            await Clipboard.WaitForChange(1000);
            Input.Paste();
            RestoreClip(data);
        }

        private static async void DelayedEnable(Task wait) {
            await wait;
            await Task.Delay(enableDelay);
            Enable();
        }

        private static async void RestoreClip(DataObject data) {
            await Task.Delay(2);
            Clipboard.SetData(data);
            await Clipboard.WaitForChange(1000);
            await Task.Delay(enableDelay);
            Enable();
        }
    }
}
