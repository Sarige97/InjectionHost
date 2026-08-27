namespace muju
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.overviewBox = new System.Windows.Forms.GroupBox();
            this.deviceStatusBox = new System.Windows.Forms.GroupBox();
            this.alertPanelBox = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // overviewBox
            // 
            this.overviewBox.Location = new System.Drawing.Point(9, 7);
            this.overviewBox.Name = "overviewBox";
            this.overviewBox.Size = new System.Drawing.Size(319, 1022);
            this.overviewBox.TabIndex = 0;
            this.overviewBox.TabStop = false;
            this.overviewBox.Text = "groupBox1";
            this.overviewBox.Enter += new System.EventHandler(this.overviewBox_Enter);
            // 
            // deviceStatusBox
            // 
            this.deviceStatusBox.Location = new System.Drawing.Point(334, 7);
            this.deviceStatusBox.Name = "deviceStatusBox";
            this.deviceStatusBox.Size = new System.Drawing.Size(1141, 1022);
            this.deviceStatusBox.TabIndex = 1;
            this.deviceStatusBox.TabStop = false;
            this.deviceStatusBox.Text = "groupBox2";
            // 
            // alertPanelBox
            // 
            this.alertPanelBox.Location = new System.Drawing.Point(1481, 7);
            this.alertPanelBox.Name = "alertPanelBox";
            this.alertPanelBox.Size = new System.Drawing.Size(415, 1022);
            this.alertPanelBox.TabIndex = 2;
            this.alertPanelBox.TabStop = false;
            this.alertPanelBox.Text = "groupBox1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.alertPanelBox);
            this.Controls.Add(this.deviceStatusBox);
            this.Controls.Add(this.overviewBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox overviewBox;
        private System.Windows.Forms.GroupBox deviceStatusBox;
        private System.Windows.Forms.GroupBox alertPanelBox;
    }
}

