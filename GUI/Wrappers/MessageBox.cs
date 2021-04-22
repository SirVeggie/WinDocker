using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Apprentice.GUI {

    public class MessageBox : GuiContainer<MessageForm> {

        public string Title => Execute(() => Form.Title);
        public string Content => Execute(() => Form.Content);

        private string title;
        private string content;

        public MessageBox(string title, string content) {
            this.title = title;
            this.content = content;
        }

        protected override MessageForm InitializeForm() => new MessageForm(title, content);
        public void SetContent(string text) => Execute(() => Form.SetContent(text));
        public void Append(string text) => Execute(() => Form.Append(text));

        // Static methods

        public static async Task Create(string text) => await Create(null, text);
        public static async Task Create(string title, string text) {
            var gui = new MessageBox(title, text);
            gui.Launch();
            await gui.WaitForForm();
            gui.Window.SetAlwaysOnTop(true);
            gui.Window.MoveTop();
            gui.Window.Activate();
            await gui.WaitForClose();
        }
    }
}
