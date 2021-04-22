using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.GUI {
    public partial class TextForm : Form {

        private static string defaultText = "(Empty)";
        public string Content => label.Text;

        protected override CreateParams CreateParams {
            get {
                CreateParams baseParams = base.CreateParams;
                baseParams.ExStyle |= (int) WS_EX.NOACTIVATE;
                return baseParams;
            }
        }

        public TextForm(string content = null, Coord? location = null) {
            InitializeComponent();
            if (location is Coord loc)
                Location = loc;
            SetContent(content);
        }

        public void SetContent(string text) => label.Text = text ?? defaultText;
        public void Append(string text) => SetContent(Content + (text ?? ""));
    }
}
