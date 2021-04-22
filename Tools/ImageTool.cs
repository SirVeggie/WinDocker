using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUtilities;

namespace Apprentice.Tools.Extensions {
    public static class ImageTool {
        public static Image Resize(this Image image, int width, int height, bool dispose = true) => Resize(image, new Coord(width, height), dispose);
        public static Image Resize(this Image image, Coord size, bool dispose = true) {
            var newImage = new Bitmap(image, size);
            if (dispose)
                image.Dispose();
            return newImage;
        }

        public static Image Resize(this Image image, double factor, bool dispose = true) {
            Coord newSize = (Coord) image.Size * factor;
            return image.Resize(newSize, dispose);
        }

        public static Image Crop(this Image image, Area cropOffsets, bool dispose = true) {
            throw new NotImplementedException();
        }

        public static Image Rotate(this Image image, double angle, bool dispose = true) {
            throw new NotImplementedException();
        }

        public static async Task SaveAs(this Image image, string file) {
            throw new NotImplementedException();
        }
    }
}
