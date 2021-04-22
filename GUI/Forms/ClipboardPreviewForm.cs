using Apprentice.GUI.Extensions;
using Apprentice.Hotkeys;
using Apprentice.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinUtilities;

namespace Apprentice.GUI {
    public partial class ClipboardPreviewForm : Form {

        private HashSet<WM> capturedMessages = new HashSet<WM> { WM.LBUTTONDOWN, WM.RBUTTONDOWN };
        private bool isCopy;

        public int MaxWidth { get; set; } = 1500;
        public int MaxHeight { get; set; } = 1000;
        public int Offset { get; set; } = 4;

        public string Desc => labelContent.Text;
        public Image Image => imageContent.Image;
        public bool HasDesc => labelContent.Visible;
        public bool HasImage => imageContent.Visible;

        public bool Grabbed { get; private set; }

        protected override CreateParams CreateParams {
            get {
                CreateParams baseParams = base.CreateParams;
                baseParams.ExStyle |= (int) WS_EX.NOACTIVATE;
                return baseParams;
            }
        }

        public ClipboardPreviewForm(string description, Coord? location = null) {
            InitializeComponent();
            textContent.Visible = false;
            labelContent.Location = new Point(Offset, Offset);
            if (location is Coord loc)
                Location = loc;
            SetDesc(description);
            CaptureClicks();

            Shown += async (o, e) => {
                this.AddShadow();
                ResetSize();
                var win = new Window(Handle);
                win.SetAlwaysOnTop(true);
                win.MoveTop();
                await Task.Delay(10);
                WinAPI.HideCaret(textContent.Handle);
            };
        }

        public ClipboardPreviewForm(Image image, string description, Coord? location = null) : this(description, location) {
            SetImage(image);

            Shown += (o, e) => {
                if (Image != null)
                    Size = Image.Size;
            };
        }

        private void CaptureClicks() {
            Application.AddMessageFilter(MessageFilterTool.Create(m => capturedMessages.Contains((WM) m.Msg), m => {
                if ((WM) m.Msg == WM.LBUTTONDOWN) {
                    StartDrag();
                } else if ((WM) m.Msg == WM.RBUTTONDOWN) {
                    StartClose();
                }
            }));
        }

        private void StartDrag() {
            Grabbed = true;
            if (HasImage)
                SetDesc(null);
            else
                SetDesc(Regex.Replace(Desc, @"^\d+: ", ""));
            //_ = A_Window.Drag(new Window(Handle), Key.LButton, false).ConfigureAwait(false);
        }

        private async void StartClose() {
            await KeyHandler.WaitKeyUp(Key.RButton);
            Close();
        }

        private void ResetSize() {
            if (HasImage) {
                Size = Image.Size;
                return;
            }

            Size = new Size(labelContent.Width + Offset * 2, labelContent.Height + Offset * 2);
        }

        public void SetDesc(string text) {
            if (string.IsNullOrEmpty(text)) {
                labelContent.Visible = false;
                return;
            } else {
                labelContent.Visible = true;
            }

            textContent.Text = text;
            labelContent.Text = text;
            ResetSize();
        }

        /// <summary>Set the image to be displayed. The old image is not disposed.</summary>
        public void SetImage(Image image) {
            if (image == null) {
                imageContent.Visible = false;
                //imageContent.Image?.Dispose();
                ResetSize();
                return;
            } else if (!imageContent.Visible) {
                imageContent.Visible = true;
            }

            var oldIsCopy = isCopy;

            var factor = Math.Max(MaxWidth / (double) image.Width, MaxHeight / (double) image.Height);
            if (factor < 1) {
                var newImage = new Bitmap(image, new Size((int) (image.Width * factor), (int) (image.Height * factor)));
                //image.Dispose();
                image = newImage;
                isCopy = true;
            }

            Size = image.Size;

            var old = imageContent.Image;
            imageContent.Image = image;
            if (oldIsCopy)
                old?.Dispose();
        }
    }
}
