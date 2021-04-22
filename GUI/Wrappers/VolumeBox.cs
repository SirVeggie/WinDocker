using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprentice.Tools;

namespace Apprentice.GUI {
    public class VolumeBox : GuiContainer<VolumeForm> {

        public float Level => (float) Execute(() => Form.Level);
        private float level;

        public VolumeBox(float level) {
            this.level = level;
        }

        protected override VolumeForm InitializeForm() => new VolumeForm(level);
        public void SetLevel(float level) => Execute(() => Form.SetLevel(level));

        // Static methods

        private static VolumeBox current;
        private static long closeTime;
        private const int timeout = 2000;

        public static async void Show(float level) {
            if (current != null) {
                closeTime = Time.Now + timeout;
                current.SetLevel(level);
                return;
            }

            current = new VolumeBox(level);
            current.Launch();
            closeTime = Time.Now + timeout;

            while (Time.Now < closeTime) {
                await Task.Delay((int) (closeTime - Time.Now));
            }

            current.Close();
            current = null;
        }
    }
}
