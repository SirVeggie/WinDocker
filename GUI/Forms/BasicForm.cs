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
    public partial class BasicForm : Form {
        public BasicForm(Area area, Color color) {
            InitializeComponent();
            Location = area;
            Size = area;
            BackColor = color;
        }
    }
}
