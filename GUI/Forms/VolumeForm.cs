using Apprentice.GUI.Extensions;
using Apprentice.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.GUI {
    public partial class VolumeForm : Form {

        public float Level;

        private Size volSize = new Size(50, 200);
        private int padding = 5;
        private int border = 2;

        private Color baseColor = Color.FromArgb(0x38, 0x30, 0x2E);
        private Color borderColor = Color.FromArgb(0x17, 0x16, 0x14);
        private Color meterColor = Color.FromArgb(0x6F, 0x68, 0x66);
        private Color volColor = Color.FromArgb(0xF1, 0xE8, 0xB8);

        public VolumeForm(float level, Coord? location = null) {
            InitializeComponent();
            Level = Matht.Clamp(level, 0, 1);
            Location = location != null ? (Coord) location : new Coord(10, 10);
            ClientSize = volSize;
            BackColor = baseColor;

            Shown += (o, e) => {
                ClientSize = volSize;
                new Window(Handle).SetOpacity(0.9).SetAlwaysOnTop(true);
            };
        }

        public void SetLevel(float level) {
            Level = Matht.Clamp(level, 0, 1);
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            Area wholeArea = new Area(new Coord(0, 0), volSize);
            Area borderArea = wholeArea.Grow(-padding);
            Area meterArea = borderArea.Grow(-border);
            Area volumeArea = meterArea;

            volumeArea.Y += volumeArea.H * (1 - Level);
            volumeArea.H *= Level;

            e.Graphics.FillRectangle(new SolidBrush(borderColor), borderArea);
            e.Graphics.FillRectangle(new SolidBrush(meterColor), meterArea);
            e.Graphics.FillRectangle(new SolidBrush(volColor), volumeArea);
        }
    }
}
