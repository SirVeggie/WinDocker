using System;

namespace Apprentice.GUI {
    public class ConsoleBox : GuiContainer<ConsoleForm> {

        public string Content => Execute(() => Form.Content);
        private string content;

        public ConsoleBox(string content = null) {
            this.content = content;
        }

        protected override ConsoleForm InitializeForm() => new ConsoleForm(content);
        public void SetContent(string text) => Execute(() => Form.SetContent(text));
        public void Append(string text) => Execute(() => Form.Append(text));
    }
}
