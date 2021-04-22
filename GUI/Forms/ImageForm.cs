using Apprentice.GUI.Extensions;
using Apprentice.Tools;
using Apprentice.Tools.Extensions;
using System;
using System.Drawing;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.GUI {
    public partial class ImageForm : Form {

        public Image Image => picture_box.Image;
        public bool Unscaled { get; set; }

        protected override CreateParams CreateParams {
            get {
                CreateParams baseParams = base.CreateParams;
                baseParams.ExStyle |= (int) WS_EX.NOACTIVATE;
                return baseParams;
            }
        }

        public ImageForm(Image image, Area? location = null, bool unscaled = false) {
            InitializeComponent();
            Unscaled = unscaled;

            if (location != null) {
                Location = (Area) location;
                Size = (Area) location;
            }

            SetImage(image);

            Shown += (o, e) => {
                if (Image != null)
                    Size = Image.Size;
                this.AddShadow();
            };
        }

        public void SetImage(Image image) {
            if (image == null) {
                picture_box.Visible = false;
                picture_box.Image?.Dispose();
                return;
            } else if (!picture_box.Visible) {
                picture_box.Visible = true;
            }

            var factor = picture_box.Width / (double) image.Width;
            if (!Unscaled && factor < 1) {
                image = image.Resize(factor);
            }

            Size = image.Size;

            var old = picture_box.Image;
            picture_box.Image = image;
            old?.Dispose();
        }
    }
}
