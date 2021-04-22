using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Apprentice.GUI {
    public partial class StringInputForm : Form {

        private TaskCompletionSource<string> source;
        private int charWidth = 6;
        private int extraWidth = 12 + 24;
        private int bottomPadding;
        private Size MinClientSize => MinimumSize - Size + ClientSize;

        public string Title => Text;
        public string Content => label1.Text;
        public string Input => textBox1.Text;
        public bool Cancelled { get; private set; }

        public StringInputForm(string title = null, string content = null, string input = null) {
            source = new TaskCompletionSource<string>();
            source.SetResult("");

            InitializeComponent();

            FormClosed += (o, e) => Cancel();
            bottomPadding = ClientSize.Height - textBox1.Location.Y - textBox1.Height;

            SetTitle(title);
            SetContent(content);
            SetInput(input);
        }

        public async Task<string> WaitForInput() {
            if (IsDisposed) {
                Cancelled = true;
                return "";
            }

            if (source.Task.IsCompleted)
                source = new TaskCompletionSource<string>();
            return await source.Task;
        }

        public void SetTitle(string text) => Text = text ?? Process.GetCurrentProcess().ProcessName;
        public void SetInput(string text) => textBox1.Text = text ?? "";
        public void SetContent(string text) {
            label1.Text = text ?? "";
            FixSize();
        }

        public void Append(string text) => SetContent(Content + (text ?? ""));

        private void OnType(object sender, EventArgs e) => FixSize();

        private void Accept() {
            Cancelled = false;
            source.TrySetResult(Input);
        }

        private void Cancel() {
            textBox1.Text = "";
            Cancelled = true;
            source.TrySetResult("");
        }

        protected override bool ProcessDialogKey(Keys keyData) {
            if (keyData == Keys.Enter) {
                Accept();
                return true;
            } else if (keyData == Keys.Escape) {
                Cancel();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        private void FixSize() {
            int width = Math.Max(CalculateDescWidth(), CalculateInputWidth());
            int height;

            if (label1.Text == "") {
                height = textBox1.Height + bottomPadding * 2;
            } else {
                height = textBox1.Height + label1.Height + bottomPadding * 3;
            }

            ClientSize = new Size(Math.Max(width, MinClientSize.Width), Math.Max(height, MinClientSize.Height));
        }

        private int CalculateDescWidth() {
            return label1.Width + label1.Location.X * 2;
        }

        private int CalculateInputWidth() {
            int stringWidth = Input.Length * charWidth;
            return stringWidth + extraWidth;
        }
    }
}
