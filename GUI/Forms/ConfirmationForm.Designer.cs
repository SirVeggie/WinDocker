namespace Apprentice.GUI {
    partial class ConfirmationForm {
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
            this.label = new System.Windows.Forms.Label();
            this.btn_accept = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.grp_buttons = new System.Windows.Forms.Panel();
            this.grp_buttons.SuspendLayout();
            this.SuspendLayout();
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label.Location = new System.Drawing.Point(12, 9);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(63, 15);
            this.label.TabIndex = 0;
            this.label.Text = "Confirm?";
            // 
            // btn_accept
            // 
            this.btn_accept.Dock = System.Windows.Forms.DockStyle.Left;
            this.btn_accept.Location = new System.Drawing.Point(0, 0);
            this.btn_accept.Name = "btn_accept";
            this.btn_accept.Size = new System.Drawing.Size(81, 37);
            this.btn_accept.TabIndex = 0;
            this.btn_accept.Text = "Yes";
            this.btn_accept.UseVisualStyleBackColor = true;
            this.btn_accept.Click += new System.EventHandler(this.Accept);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btn_cancel.Location = new System.Drawing.Point(124, 0);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(81, 37);
            this.btn_cancel.TabIndex = 1;
            this.btn_cancel.Text = "No";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.Deny);
            // 
            // grp_buttons
            // 
            this.grp_buttons.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.grp_buttons.Controls.Add(this.btn_accept);
            this.grp_buttons.Controls.Add(this.btn_cancel);
            this.grp_buttons.Location = new System.Drawing.Point(12, 43);
            this.grp_buttons.Name = "grp_buttons";
            this.grp_buttons.Size = new System.Drawing.Size(205, 37);
            this.grp_buttons.TabIndex = 2;
            // 
            // ConfirmationForm
            // 
            this.AcceptButton = this.btn_accept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(229, 92);
            this.Controls.Add(this.grp_buttons);
            this.Controls.Add(this.label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfirmationForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Confirmation Box";
            this.grp_buttons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label;
        private System.Windows.Forms.Button btn_accept;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Panel grp_buttons;
    }
}