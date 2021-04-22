using Apprentice.GUI.Extensions;
using Apprentice.Hotkeys;
using Apprentice.Tools;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.GUI {
    public partial class MessageForm : Form {

        private static int padding;
        public static Size MinClientSize;
        public static Size MaxClientSize;
        private string defaultTitle = Process.GetCurrentProcess().ProcessName;

        public string Title => Text;
        public string Content => richTextBox1.Text;
        public Font TextFont => richTextBox1.Font;

        public MessageForm(string title = null, string content = null) {
            InitializeComponent();

            padding = richTextBox1.Location.X;
            MinClientSize = new Size(200, GuiTool.CalculateTextSize(Content, TextFont).Height + padding * 2);
            MaxClientSize = new Size(1600, 1000);
            MinimumSize = this.ClientToFull(MinClientSize);
            MaximumSize = this.ClientToFull(MaxClientSize);

            richTextBox1.TextChanged += (o, e) => richTextBox1.ScrollToBottom();
            richTextBox1.MouseWheel += (o, e) => richTextBox1.Scroll(e.Delta, false);

            richTextBox1.MouseDown += (o, e) => WinAPI.HideCaret(richTextBox1.Handle);
            richTextBox1.GotFocus += (o, e) => WinAPI.HideCaret(richTextBox1.Handle);
            richTextBox1.MouseWheel += (o, e) => WinAPI.HideCaret(richTextBox1.Handle);

            SetTitle(title);
            SetContent(content);
            Location = new Area(size: Size).CenterOn(Monitor.Primary.WorkArea);

            Application.AddMessageFilter(MessageFilterTool.Create(m => (WM) m.Msg == WM.CHAR && !KeyHandler.IsDown(Key.LCtrl), m => Close()));
        }

        public void SetTitle(string title) {
            Text = string.IsNullOrEmpty(title) ? defaultTitle : title;
        }

        public void SetContent(string text) {
            richTextBox1.Text = text == "" ? "(Empty)" : text ?? "(Null)";
            Size textsize = GuiTool.CalculateTextSize(Content, TextFont, MaxClientSize.Width);
            ClientSize = new Size(textsize.Width + padding * 2, Math.Min(textsize.Height + padding * 2, MaxClientSize.Height));
        }

        public void Append(string text) => SetContent(Content + (text ?? ""));
    }
}
