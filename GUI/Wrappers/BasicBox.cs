using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.GUI {
    public class BasicBox : GuiContainer<BasicForm> {

        private Area area;
        private Color color;

        public BasicBox(Area area, Color color) {
            this.area = area;
            this.color = color;
        }

        protected override BasicForm InitializeForm() => new BasicForm(area, color);
        public void SetColor(Color color) => Execute(() => Form.BackColor = color);
    }
}
