using System;
using System.Threading.Tasks;

namespace Apprentice.GUI {

    public class InputBox : GuiContainer<StringInputForm> {

        public string Title => Execute(() => Form.Title);
        public string Content => Execute(() => Form.Content);
        public string Input => Execute(() => Form.Input);
        public bool Cancelled => Form.Cancelled;

        private string title;
        private string content;
        private string input;

        public InputBox(string title = null, string content = null, string input = null) {
            this.title = title;
            this.content = content;
            this.input = input;
        }

        protected override StringInputForm InitializeForm() => new StringInputForm(title, content, input);
        public void SetContent(string text) => Execute(() => Form.SetContent(text));
        public void Append(string text) => Execute(() => Form.Append(text));
        public void SetInput(string text) => Execute(() => Form.SetInput(text));
        public async Task<string> WaitForInput() {
            await WaitForForm();
            return await Form.WaitForInput();
        }

        // Static methods
        public static Task<string> Create(string desc = null) => Create(null, desc, null);
        public static async Task<string> Create(string title, string desc, string input = null) {
            var gui = new InputBox(title, desc, input);
            gui.Launch();
            await gui.WaitForForm();
            gui.Window.SetAlwaysOnTop(true);
            gui.Window.MoveTop();
            gui.Window.Activate();
            var res = await gui.WaitForInput();
            gui.Close();
            return res;
        }
    }
}
