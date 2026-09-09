namespace moju.UI
{
    partial class WelcomePage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.loginControl = new moju.UI.LoginControl();
            this.SuspendLayout();
            // 
            // loginControl
            // 
            this.loginControl.Location = new System.Drawing.Point(12, 12);
            this.loginControl.Name = "loginControl";
            this.loginControl.Size = new System.Drawing.Size(318, 250);
            this.loginControl.TabIndex = 0;
            this.loginControl.Load += new System.EventHandler(this.loginControl_Load);
            // 
            // WelcomePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 274);
            this.Controls.Add(this.loginControl);
            this.Name = "WelcomePage";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private LoginControl loginControl;
    }
}