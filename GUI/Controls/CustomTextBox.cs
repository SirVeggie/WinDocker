using Apprentice.GUI.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.GUI {
    public class CustomTextBox : RichTextBox {

        public bool HideCaret { get; set; } = true;
        public bool Scroll { get; set; } = true;
        public bool ScrollBottom { get; set; }

        public void Initialize(bool caret, bool scroll, bool scrollBottom) {
            HideCaret = !caret;
            Scroll = scroll;
            ScrollBottom = scrollBottom;

            BackColor = SystemColors.Control;
            BorderStyle = BorderStyle.None;
            Name = "customTextBox";
            ReadOnly = true;
            ScrollBars = RichTextBoxScrollBars.None;
            Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point, 238);
            TabStop = false;
            Text = "Sample starter text";
            Location = new Point(0, 0);
            Size = new Size(100, 100);
        }

        protected override void OnTextChanged(EventArgs e) {
            if (ScrollBottom)
                this.ScrollToBottom();
            base.OnTextChanged(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            if (HideCaret)
                WinAPI.HideCaret(Handle);
            base.OnMouseDown(e);
        }

        protected override void OnGotFocus(EventArgs e) {
            if (HideCaret)
                WinAPI.HideCaret(Handle);
            base.OnGotFocus(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e) {
            if (HideCaret)
                WinAPI.HideCaret(Handle);
            if (Scroll)
                this.Scroll(e.Delta, false);
            base.OnMouseWheel(e);
        }
    }
}
