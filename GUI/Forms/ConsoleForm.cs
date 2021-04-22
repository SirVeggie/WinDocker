using Apprentice.GUI.Extensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.GUI {

    public partial class ConsoleForm : Form {

        public string Content => richTextBox1.Text;
        public readonly int lineHeight = 15;

        [DllImport("user32.dll")]
        private static extern bool HideCaret(IntPtr hwnd);

        public ConsoleForm(string content = null) {
            InitializeComponent();
            Text = Process.GetCurrentProcess().ProcessName;

            richTextBox1.TextChanged += (o, e) => richTextBox1.ScrollToBottom();
            richTextBox1.MouseDown += (o, e) => WinAPI.HideCaret(richTextBox1.Handle);
            richTextBox1.GotFocus += (o, e) => WinAPI.HideCaret(richTextBox1.Handle);
            richTextBox1.MouseWheel += (o, e) => richTextBox1.Scroll(e.Delta, false);

            richTextBox1.MouseWheel += (o, e) => UpdateScrollIndicator();
            richTextBox1.TextChanged += (o, e) => UpdateScrollIndicator();
            richTextBox1.MouseWheel += (o, e) => WinAPI.HideCaret(richTextBox1.Handle);

            SetContent(content);
        }

        public void SetContent(string text) => richTextBox1.Text = text ?? "";
        public void Append(string text) => richTextBox1.AppendText(text ?? "");

        private void UpdateScrollIndicator() {
            int index = richTextBox1.GetCharIndexFromPosition(new Point(3, 4));
            int line = richTextBox1.GetLineFromCharIndex(index) + 1;
            int linesOnScreen = richTextBox1.Height / lineHeight;
            int lineCount = richTextBox1.Lines.Length - 1;

            infolabel.Text = $"Lines: {line}-{Math.Min(line + linesOnScreen - 1, lineCount)} / {lineCount}";
        }
    }
}
