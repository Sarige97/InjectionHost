namespace moju.UI
{
    partial class StandardDoubleLabel
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.LeftLabel = new System.Windows.Forms.Label();
            this.MiddleLabel = new System.Windows.Forms.Label();
            this.RightLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LeftLabel
            // 
            this.LeftLabel.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LeftLabel.Location = new System.Drawing.Point(0, 0);
            this.LeftLabel.Name = "LeftLabel";
            this.LeftLabel.Size = new System.Drawing.Size(180, 30);
            this.LeftLabel.TabIndex = 0;
            this.LeftLabel.Text = "LeftLabel";
            this.LeftLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MiddleLabel
            // 
            this.MiddleLabel.AutoSize = true;
            this.MiddleLabel.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MiddleLabel.Location = new System.Drawing.Point(172, 2);
            this.MiddleLabel.Margin = new System.Windows.Forms.Padding(0);
            this.MiddleLabel.Name = "MiddleLabel";
            this.MiddleLabel.Size = new System.Drawing.Size(17, 26);
            this.MiddleLabel.TabIndex = 1;
            this.MiddleLabel.Text = ":";
            this.MiddleLabel.Click += new System.EventHandler(this.label2_Click);
            // 
            // RightLabel
            // 
            this.RightLabel.AutoSize = true;
            this.RightLabel.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RightLabel.Location = new System.Drawing.Point(186, 2);
            this.RightLabel.Name = "RightLabel";
            this.RightLabel.Size = new System.Drawing.Size(109, 25);
            this.RightLabel.TabIndex = 2;
            this.RightLabel.Text = "RightLabel";
            this.RightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.RightLabel.Click += new System.EventHandler(this.RightLabel_Click);
            // 
            // StandardDoubleLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RightLabel);
            this.Controls.Add(this.MiddleLabel);
            this.Controls.Add(this.LeftLabel);
            this.Name = "StandardDoubleLabel";
            this.Size = new System.Drawing.Size(651, 30);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LeftLabel;
        private System.Windows.Forms.Label MiddleLabel;
        private System.Windows.Forms.Label RightLabel;
    }
}
