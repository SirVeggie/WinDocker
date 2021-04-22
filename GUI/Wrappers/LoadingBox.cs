using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprentice.GUI {
    public class LoadingBox : GuiContainer<LoadingForm> {

        public string Title => Execute(() => Form.Title);
        public string Content => Execute(() => Form.Content);
        public double Progress => (double) Execute(() => Form.Progress);
        public int Max => Execute(() => Form.Max);
        public int Value => Execute(() => Form.Value);

        private string title;
        private string content;

        public LoadingBox(string title = null, string content = null) {
            this.title = title;
            this.content = content;
        }

        protected override LoadingForm InitializeForm() => new LoadingForm(title, content);
        public void SetTitle(string title) => Execute(() => Form.SetTitle(title));
        public void SetContent(string content) => Execute(() => Form.SetContent(content));
        public double SetProgress(double progress) => (double) Execute(() => Form.SetProgress(progress));
        public void SetMax(int value) => Execute(() => Form.SetMax(value));
        public void SetValue(int value) => Execute(() => Form.SetValue(value));
        public void AddValue(int value) => Execute(() => Form.AddValue(value));
    }
}
