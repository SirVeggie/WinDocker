namespace Apprentice.GUI {
    partial class NotificationForm {
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
            this.label_title = new System.Windows.Forms.Label();
            this.label_content = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label_title
            // 
            this.label_title.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label_title.Dock = System.Windows.Forms.DockStyle.Top;
            this.label_title.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label_title.ForeColor = System.Drawing.SystemColors.Control;
            this.label_title.Location = new System.Drawing.Point(0, 0);
            this.label_title.Margin = new System.Windows.Forms.Padding(0);
            this.label_title.Name = "label_title";
            this.label_title.Padding = new System.Windows.Forms.Padding(10);
            this.label_title.Size = new System.Drawing.Size(400, 40);
            this.label_title.TabIndex = 0;
            this.label_title.Text = "Notification Title";
            // 
            // label_content
            // 
            this.label_content.AutoSize = true;
            this.label_content.BackColor = System.Drawing.SystemColors.Control;
            this.label_content.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label_content.Location = new System.Drawing.Point(12, 53);
            this.label_content.Margin = new System.Windows.Forms.Padding(0);
            this.label_content.Name = "label_content";
            this.label_content.Size = new System.Drawing.Size(336, 18);
            this.label_content.TabIndex = 1;
            this.label_content.Text = "This is some content for the notification";
            // 
            // NotificationForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(400, 130);
            this.Controls.Add(this.label_content);
            this.Controls.Add(this.label_title);
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "NotificationForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "NotificationForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_title;
        private System.Windows.Forms.Label label_content;
    }
}