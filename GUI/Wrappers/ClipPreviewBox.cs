using Apprentice.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.GUI {
    public class ClipPreviewBox : GuiContainer<ClipboardPreviewForm> {

        private string description;
        private Image image;
        private Coord? location;
        private bool hasImage;

        public int MaxWidth => Execute(() => Form.MaxWidth);
        public int MaxHeight => Execute(() => Form.MaxHeight);
        public int Offset => Execute(() => Form.Offset);
        public string Desc => Execute(() => Form.Desc);
        public Image Image => Execute(() => Form.Image);
        public bool HasDesc => Execute(() => Form.HasDesc);
        public bool HasImage => Execute(() => Form.HasImage);
        public bool Grabbed => Execute(() => Form.Grabbed);

        public ClipPreviewBox(string description, Coord? location = null) {
            this.location = location;
            this.description = description;
        }

        public ClipPreviewBox(Image image, string description, Coord? location = null) : this(description, location) {
            this.image = image;
            hasImage = true;
        }

        protected override ClipboardPreviewForm InitializeForm() {
            if (hasImage)
                return new ClipboardPreviewForm(image, description, location);
            return new ClipboardPreviewForm(description, location);
        }

        public void SetDesc(string text) => Execute(() => Form.SetDesc(text));
        public void SetImage(Image image) => Execute(() => Form.SetImage(image));
    }
}
