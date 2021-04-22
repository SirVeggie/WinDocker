using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Apprentice.GUI {

    public partial class ConfirmationForm : Form {

        private TaskCompletionSource<bool> source;
        private static string defaultText = "Are you sure?";

        public bool LastResult { get; private set; }
        public string Title => Text;
        public string Content => label.Text;
        public bool Cancelled { get; private set; }

        public ConfirmationForm(string title = null, string content = null) {
            source = new TaskCompletionSource<bool>();
            InitializeComponent();
            FormClosed += (o, e) => Cancel();
            SetTitle(title);
            SetContent(content);
        }

        public async Task<bool> WaitForInput() {
            if (IsDisposed) {
                Cancelled = true;
                return false;
            }

            if (source.Task.IsCompleted)
                source = new TaskCompletionSource<bool>();
            return await source.Task;
        }

        public void SetTitle(string text) {
            Text = text ?? Process.GetCurrentProcess().ProcessName;
        }

        public void SetContent(string text) {
            label.Text = text ?? defaultText;
            int width = Math.Max(label.Width, grp_buttons.Width) + label.Location.X * 2;
            int height = grp_buttons.Height + label.Height + label.Location.Y * 3;
            ClientSize = new Size(width, height);
        }

        public void Append(string text) => SetContent(Content + (text ?? ""));

        private void Accept(object sender, EventArgs e) {
            LastResult = true;
            Cancelled = false;
            source.TrySetResult(true);
        }

        private void Deny(object sender, EventArgs e) {
            LastResult = false;
            Cancelled = false;
            source.TrySetResult(false);
        }

        private void Cancel() {
            LastResult = false;
            Cancelled = true;
            source.TrySetResult(false);
        }

        protected override bool ProcessDialogKey(Keys keyData) {
            if (keyData == Keys.Escape) {
                Cancel();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }
    }
}
