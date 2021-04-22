using System;
using System.Drawing;
using WinUtilities;

namespace Apprentice.GUI {
    public class ImageBox : GuiContainer<ImageForm> {

        public Image Image => Execute(() => Form.Image);
        public bool Unscaled {
            get => Execute(() => Form.Unscaled);
            set => Execute(() => Form.Unscaled = value);
        }

        private Image image;
        private Area? location;
        private bool unscaled;

        public ImageBox(Image image, Area? location = null, bool unscaled = false) {
            this.image = image;
            this.location = location;
            this.unscaled = unscaled;
        }

        protected override ImageForm InitializeForm() => new ImageForm(image, location, unscaled);
        public void SetImage(Image image) => Execute(() => Form.SetImage(image));
    }
}
