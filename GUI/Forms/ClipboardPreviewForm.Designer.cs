
namespace Apprentice.GUI {
    partial class ClipboardPreviewForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.imageContent = new System.Windows.Forms.PictureBox();
            this.labelContent = new System.Windows.Forms.Label();
            this.textContent = new Apprentice.GUI.CustomTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.imageContent)).BeginInit();
            this.SuspendLayout();
            // 
            // imageContent
            // 
            this.imageContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageContent.Location = new System.Drawing.Point(0, 0);
            this.imageContent.Name = "imageContent";
            this.imageContent.Size = new System.Drawing.Size(231, 148);
            this.imageContent.TabIndex = 1;
            this.imageContent.TabStop = false;
            this.imageContent.Visible = false;
            // 
            // labelContent
            // 
            this.labelContent.AutoSize = true;
            this.labelContent.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelContent.Location = new System.Drawing.Point(8, 8);
            this.labelContent.Name = "labelContent";
            this.labelContent.Padding = new System.Windows.Forms.Padding(4, 4, 2, 0);
            this.labelContent.Size = new System.Drawing.Size(79, 17);
            this.labelContent.TabIndex = 2;
            this.labelContent.Text = "sample text";
            // 
            // textContent
            // 
            this.textContent.BackColor = System.Drawing.SystemColors.Control;
            this.textContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textContent.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textContent.HideCaret = true;
            this.textContent.Location = new System.Drawing.Point(68, 43);
            this.textContent.Margin = new System.Windows.Forms.Padding(0);
            this.textContent.Name = "textContent";
            this.textContent.ReadOnly = true;
            this.textContent.Scroll = true;
            this.textContent.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.textContent.ScrollBottom = false;
            this.textContent.Size = new System.Drawing.Size(100, 96);
            this.textContent.TabIndex = 3;
            this.textContent.Text = "Sample text";
            // 
            // ClipboardPreviewForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(231, 148);
            this.Controls.Add(this.textContent);
            this.Controls.Add(this.labelContent);
            this.Controls.Add(this.imageContent);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ClipboardPreviewForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "ClipboardPreviewForm";
            ((System.ComponentModel.ISupportInitialize)(this.imageContent)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox imageContent;
        private System.Windows.Forms.Label labelContent;
        private CustomTextBox textContent;
    }
}