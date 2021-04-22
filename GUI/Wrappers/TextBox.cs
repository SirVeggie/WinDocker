using System;
using WinUtilities;

namespace Apprentice.GUI {

    public class TextBox : GuiContainer<TextForm> {

        public string Content => Execute(() => Form.Content);
        private string content;
        private Coord? location;

        public TextBox(string content, Coord? location = null) {
            this.content = content;
            this.location = location;
        }

        protected override TextForm InitializeForm() => new TextForm(content, location);
        public void SetContent(string text) => Execute(() => Form.SetContent(text));
        public void Append(string text) => Execute(() => Form.Append(text));
    }
}
