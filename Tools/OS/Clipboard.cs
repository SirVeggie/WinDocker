using Apprentice.Debugging;
using Apprentice.GUI;
using Apprentice.Tools.Extensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Apprentice.Tools {
    public static class Clipboard {

        public static event Action<ClipboardData> Updated;
        private static ClipboardMonitor monitor;
        private const int WM_CLIPBOARDUPDATE = 0x31D;
        private static AsyncGate<ClipboardData> gate;
        private static readonly string[] clipboardMetaFormats = { "application/x-moz-nativeimage", "FileContents", "EnhancedMetafile", "System.Drawing.Imaging.Metafile", "MetaFilePict", "Object Descriptor", "ObjectLink", "Link Source Descriptor", "Link Source", "Embed Source", "Hyperlink" };

        static Clipboard() {
            gate = new AsyncGate<ClipboardData>();
            Updated += data => gate.Release(data);
        }

        public static void Clear() => new Thread(() => System.Windows.Forms.Clipboard.Clear()).RunStaJoin();
        public static void SetText(string text) => new Thread(() => System.Windows.Forms.Clipboard.SetText(text)).RunStaJoin();
        public static void SetImage(Image image) => new Thread(() => System.Windows.Forms.Clipboard.SetImage(image)).RunStaJoin();
        public static void SetData(string format, object data) => new Thread(() => System.Windows.Forms.Clipboard.SetData(format, data)).RunStaJoin();
        public static bool SetData(IDataObject data) {
            if (data == null)
                throw new ArgumentNullException("Data was null, to clear clipboard use Clear() instead");
            bool success = true;

            new Thread(() => {
                try {
                    System.Windows.Forms.Clipboard.SetDataObject(data, true);
                } catch (ExternalException e) {
                    success = false;
                    Console.WriteLine($"Error {e.ErrorCode}: {e.Message}");
                }
            }).RunStaJoin();

            return success;
        }

        public static string GetText() {
            string text = null;
            new Thread(() => text = System.Windows.Forms.Clipboard.GetText()).RunStaJoin();
            return text;
        }

        public static Image GetImage() {
            Image image = null;
            new Thread(() => image = System.Windows.Forms.Clipboard.GetImage()).RunStaJoin();
            return image;
        }

        public static object GetData(string format) {
            object data = null;
            new Thread(() => data = System.Windows.Forms.Clipboard.GetData(format)).RunStaJoin();
            return data;
        }

        public static DataObject GetData() {
            DataObject data = null;
            new Thread(() => data = CopyData(System.Windows.Forms.Clipboard.GetDataObject())).RunStaJoin();
            return data;
        }

        public static bool ContainsText() {
            bool contains = false;
            new Thread(() => contains = System.Windows.Forms.Clipboard.ContainsText()).RunStaJoin();
            return contains;
        }

        public static bool ContainsImage() {
            bool contains = false;
            new Thread(() => contains = System.Windows.Forms.Clipboard.ContainsImage()).RunStaJoin();
            return contains;
        }

        public static bool ContainsData(string format) {
            bool contains = false;
            new Thread(() => contains = System.Windows.Forms.Clipboard.ContainsData(format)).RunStaJoin();
            return contains;
        }

        public static bool ContainsData() {
            bool contains = false;
            new Thread(() => contains = System.Windows.Forms.Clipboard.GetDataObject() != null).RunStaJoin();
            return contains;
        }

        public static Task<ClipboardData> WaitForChange() => gate.Wait();
        public static Task<Job<ClipboardData>> WaitForChange(int timeout) => gate.Wait(timeout);

        private static DataObject CopyData(IDataObject obj) {
            DataObject result = new DataObject();
            string[] formats = obj.GetFormats()?.Except(clipboardMetaFormats).ToArray() ?? Array.Empty<string>();
            foreach (string format in formats) {
                try {
                    object data = obj.GetData(format);
                    if (data != null) result.SetData(format, data);
                } catch (ExternalException ex) {
                    Console.WriteLine($"Error {ex.ErrorCode}: {ex.Message}");
                }
            }
            return result;
        }

        /// <summary>Subscribe to clipboard change events</summary>
        /// <remarks>Must be registered in a thread that doesn't terminate and has a message loop</remarks>
        public static void RegisterClipboardMessage() {
            monitor = new ClipboardMonitor();
            AddClipboardFormatListener(monitor.Handle);
        }

        private class ClipboardMonitor : Form {
            protected override void WndProc(ref Message m) {
                if (m.Msg == WM_CLIPBOARDUPDATE)
                    Updated?.Invoke(new ClipboardData(GetData()));
                base.WndProc(ref m);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
    }

    public class ClipboardData {
        public DataObject Data { get; private set; }

        public bool HasText { get; }
        public bool HasImage { get; }
        public bool IsEmpty => Data == null;

        public ClipboardData(DataObject data) {
            Data = data;
            HasText = Data.GetDataPresent(DataFormats.UnicodeText);
            HasImage = Data.GetDataPresent(DataFormats.Bitmap);
        }

        public string GetText() => HasText ? (string) Data.GetData(DataFormats.UnicodeText) : null;
        public Image GetImage() => HasImage ? (Image) Data.GetData(DataFormats.Bitmap) : null;

        public override string ToString() => HasText ? GetText() : $"{{ Clipboard: {Data.GetFormats().StringJoin(" | ")} }}";
    }
}
