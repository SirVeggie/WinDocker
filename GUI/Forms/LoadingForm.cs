using Apprentice.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.GUI {
    public partial class LoadingForm : Form {

        public string Title => Text;
        public string Content => label1.Text;
        public double Progress => progressBar1.Value / progressBar1.Maximum;
        public int Max => progressBar1.Maximum;
        public int Value => progressBar1.Value;

        public LoadingForm(string title = null, string content = null) {
            InitializeComponent();

            SetTitle(title);
            SetContent(content);

            Location = new Area(size: Size).CenterOn(Monitor.Primary.WorkArea);
            Shown += (o, e) => {
                var win = new Window(Handle);
                win.SetAlwaysOnTop(true);
                win.MoveTop();
                win.Activate();
            };
        }

        public void SetTitle(string title) {
            Text = title ?? Process.GetCurrentProcess().ProcessName;
        }

        public void SetContent(string text) {
            label1.Text = text ?? "";
        }

        public double SetProgress(double progress) {
            progressBar1.Value = (int) Math.Round(progress * progressBar1.Maximum);
            return Progress;
        }

        public void SetMax(int value) {
            progressBar1.Maximum = value;
        }

        public void SetValue(int value) {
            progressBar1.Value = Matht.Clamp(value, 0, Max);
        }

        public void AddValue(int value) {
            progressBar1.Value += value;
        }

        /// <summary>Default is 100 ms</summary>
        public void SetAnimationSpeed(int speed) {
            progressBar1.MarqueeAnimationSpeed = speed;
        }
    }
}
