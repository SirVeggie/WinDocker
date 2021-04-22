using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.GUI {
    public partial class NotificationForm : Form {

        public string Title => label_title.Text;
        public string Content => label_content.Text;
        private Action LeftAction { get; set; }
        private Action RightAction { get; set; }

        private int fadeDuration = 200;
        private int fadeResolution = 20;

        private static int minWidth = 250;
        private static int maxWidth = 1500;
        private static int maxHeight = 1000;

        protected override CreateParams CreateParams {
            get {
                CreateParams baseParams = base.CreateParams;
                baseParams.ExStyle |= (int) WS_EX.NOACTIVATE;
                return baseParams;
            }
        }

        public NotificationForm(string title, string content, Action leftClick = null, Action rightClick = null) {
            InitializeComponent();
            Location = new Point(-10000, -10000);
            LeftAction = SlowClose + leftClick;
            RightAction = SlowClose + rightClick;
            SetTitle(title);
            SetContent(content ?? "");
            Application.AddMessageFilter(new KeyFilter(LeftAction, RightAction));
        }

        public void SetTitle(string text) {
            label_title.Text = text ?? "Notice";
        }

        public void SetContent(string text) {
            label_content.Text = text;
            ClientSize = GetSize();
        }

        public void SetColors(Color? title = null, Color? titleBack = null, Color? desc = null, Color? descBack = null) {
            if (title != null)
                label_title.ForeColor = (Color) title;
            if (titleBack != null)
                label_title.BackColor = (Color) titleBack;
            if (desc != null)
                label_content.ForeColor = (Color) desc;
            if (descBack != null)
                label_content.BackColor = (Color) descBack;
        }

        public void SetFontSize(float size) {
            label_content.Font = new Font(label_content.Font.FontFamily, size);
            ClientSize = GetSize();
        }

        private Size GetSize() {
            var contentPad = label_content.Location.X;
            var width = label_content.Width + contentPad * 2;
            var height = string.IsNullOrEmpty(label_content.Text) ? label_title.Height : label_content.Location.Y + label_content.Height + contentPad;
            return new Size(Math.Min(Math.Max(minWidth, width), maxWidth), Math.Min(height, maxHeight));
        }

        public async void SlowClose() {
            var win = new Window(Handle);
            double opacity = 1;
            win.SetClickThrough(true);

            for (int i = 0; i < fadeResolution - 1; i++) {
                opacity -= 1.0 / fadeResolution;
                win.SetOpacity(opacity);
                await Task.Delay(fadeDuration / fadeResolution);
            }

            Close();
        }

        private class KeyFilter : IMessageFilter {
            private Action lclick;
            private Action rclick;

            public KeyFilter(Action lclick, Action rclick) {
                this.lclick = lclick;
                this.rclick = rclick;
            }

            public bool PreFilterMessage(ref Message m) {
                var msg = (WM) m.Msg;
                if (msg == WM.LBUTTONDOWN) {
                    lclick?.Invoke();
                } else if (msg == WM.RBUTTONDOWN) {
                    rclick?.Invoke();
                }

                return false;
            }
        }
    }
}
