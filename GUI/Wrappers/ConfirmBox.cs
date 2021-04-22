using System;
using System.Threading.Tasks;

namespace Apprentice.GUI {

    public class ConfirmBox : GuiContainer<ConfirmationForm> {

        public bool LastResult => Form.LastResult;
        public string Title => Execute(() => Form.Title);
        public string Content => Execute(() => Form.Content);
        public bool Cancelled => Form.Cancelled;

        private string title;
        private string content;

        public ConfirmBox(string title = null, string content = null) {
            this.title = title;
            this.content = content;
        }

        protected override ConfirmationForm InitializeForm() => new ConfirmationForm(title, content);
        public void SetTitle(string text) => Execute(() => Form.SetTitle(text));
        public void SetContent(string text) => Execute(() => Form.SetContent(text));
        public void Append(string text) => Execute(() => Form.Append(text));
        public async Task<bool> WaitForInput() {
            await WaitForForm();
            return await Form.WaitForInput();
        }

        // Static methods

        public static Task<bool> Create(string desc = null) => Create(null, desc);
        public static async Task<bool> Create(string title, string desc = null) {
            var gui = new ConfirmBox(title, desc);
            await gui.LaunchAndWait();
            gui.Window.SetAlwaysOnTop(true);
            gui.Window.MoveTop();
            await Task.Delay(100);
            gui.Window.Activate();
            var res = await gui.WaitForInput();
            gui.Close();
            return res;
        }
    }
}
