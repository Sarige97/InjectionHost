namespace muju
{
    partial class MainForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.LogPanel = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.Overview = new System.Windows.Forms.Label();
            this.MainPanel = new System.Windows.Forms.Panel();
            this.OverviewPanel = new System.Windows.Forms.Panel();
            this.LabelBoxWorkshopTemperature = new moju.UI.StandardDoubleLabel();
            this.LabelBoxWorkshopHumidity = new moju.UI.StandardDoubleLabel();
            this.LabelBoxWorkshopDewPoint = new moju.UI.StandardDoubleLabel();
            this.LabelBoxWorkshopNoiseLevel = new moju.UI.StandardDoubleLabel();
            this.OverviewButtonHvacTempSetPoint = new System.Windows.Forms.Button();
            this.OverviewButtonHumiditySetPoint = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel5.SuspendLayout();
            this.OverviewPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.LogPanel);
            this.panel1.Controls.Add(this.panel5);
            this.panel1.Controls.Add(this.MainPanel);
            this.panel1.Controls.Add(this.OverviewPanel);
            this.panel1.Location = new System.Drawing.Point(2, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1900, 1036);
            this.panel1.TabIndex = 0;
            // 
            // LogPanel
            // 
            this.LogPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LogPanel.Location = new System.Drawing.Point(220, 812);
            this.LogPanel.Name = "LogPanel";
            this.LogPanel.Size = new System.Drawing.Size(1669, 215);
            this.LogPanel.TabIndex = 0;
            // 
            // panel5
            // 
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel5.Controls.Add(this.Overview);
            this.panel5.Location = new System.Drawing.Point(10, 10);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(204, 105);
            this.panel5.TabIndex = 0;
            // 
            // Overview
            // 
            this.Overview.AutoSize = true;
            this.Overview.BackColor = System.Drawing.SystemColors.Control;
            this.Overview.Font = new System.Drawing.Font("微软雅黑", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Overview.ForeColor = System.Drawing.SystemColors.WindowFrame;
            this.Overview.Location = new System.Drawing.Point(49, 24);
            this.Overview.Name = "Overview";
            this.Overview.Size = new System.Drawing.Size(90, 46);
            this.Overview.TabIndex = 0;
            this.Overview.Text = "总览";
            // 
            // MainPanel
            // 
            this.MainPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MainPanel.Location = new System.Drawing.Point(220, 10);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(1669, 796);
            this.MainPanel.TabIndex = 0;
            // 
            // OverviewPanel
            // 
            this.OverviewPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OverviewPanel.Controls.Add(this.OverviewButtonHumiditySetPoint);
            this.OverviewPanel.Controls.Add(this.OverviewButtonHvacTempSetPoint);
            this.OverviewPanel.Controls.Add(this.LabelBoxWorkshopNoiseLevel);
            this.OverviewPanel.Controls.Add(this.LabelBoxWorkshopDewPoint);
            this.OverviewPanel.Controls.Add(this.LabelBoxWorkshopHumidity);
            this.OverviewPanel.Controls.Add(this.LabelBoxWorkshopTemperature);
            this.OverviewPanel.Location = new System.Drawing.Point(10, 10);
            this.OverviewPanel.Name = "OverviewPanel";
            this.OverviewPanel.Size = new System.Drawing.Size(204, 1017);
            this.OverviewPanel.TabIndex = 0;
            // 
            // LabelBoxWorkshopTemperature
            // 
            this.LabelBoxWorkshopTemperature.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LabelBoxWorkshopTemperature.LeftLabelText = "车间温度";
            this.LabelBoxWorkshopTemperature.Location = new System.Drawing.Point(-83, 110);
            this.LabelBoxWorkshopTemperature.Name = "LabelBoxWorkshopTemperature";
            this.LabelBoxWorkshopTemperature.RightLabelText = "0℃";
            this.LabelBoxWorkshopTemperature.Size = new System.Drawing.Size(303, 28);
            this.LabelBoxWorkshopTemperature.TabIndex = 0;
            // 
            // LabelBoxWorkshopHumidity
            // 
            this.LabelBoxWorkshopHumidity.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LabelBoxWorkshopHumidity.LeftLabelText = "车间湿度";
            this.LabelBoxWorkshopHumidity.Location = new System.Drawing.Point(-83, 144);
            this.LabelBoxWorkshopHumidity.Name = "LabelBoxWorkshopHumidity";
            this.LabelBoxWorkshopHumidity.RightLabelText = "0%";
            this.LabelBoxWorkshopHumidity.Size = new System.Drawing.Size(303, 28);
            this.LabelBoxWorkshopHumidity.TabIndex = 1;
            // 
            // LabelBoxWorkshopDewPoint
            // 
            this.LabelBoxWorkshopDewPoint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LabelBoxWorkshopDewPoint.LeftLabelText = "露点";
            this.LabelBoxWorkshopDewPoint.Location = new System.Drawing.Point(-83, 178);
            this.LabelBoxWorkshopDewPoint.Name = "LabelBoxWorkshopDewPoint";
            this.LabelBoxWorkshopDewPoint.RightLabelText = "0℃";
            this.LabelBoxWorkshopDewPoint.Size = new System.Drawing.Size(303, 28);
            this.LabelBoxWorkshopDewPoint.TabIndex = 2;
            // 
            // LabelBoxWorkshopNoiseLevel
            // 
            this.LabelBoxWorkshopNoiseLevel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LabelBoxWorkshopNoiseLevel.LeftLabelText = "噪音";
            this.LabelBoxWorkshopNoiseLevel.Location = new System.Drawing.Point(-83, 212);
            this.LabelBoxWorkshopNoiseLevel.Name = "LabelBoxWorkshopNoiseLevel";
            this.LabelBoxWorkshopNoiseLevel.RightLabelText = "0℃";
            this.LabelBoxWorkshopNoiseLevel.Size = new System.Drawing.Size(303, 28);
            this.LabelBoxWorkshopNoiseLevel.TabIndex = 3;
            // 
            // OverviewButtonHvacTempSetPoint
            // 
            this.OverviewButtonHvacTempSetPoint.BackColor = System.Drawing.SystemColors.Control;
            this.OverviewButtonHvacTempSetPoint.Location = new System.Drawing.Point(19, 246);
            this.OverviewButtonHvacTempSetPoint.Name = "OverviewButtonHvacTempSetPoint";
            this.OverviewButtonHvacTempSetPoint.Size = new System.Drawing.Size(161, 30);
            this.OverviewButtonHvacTempSetPoint.TabIndex = 4;
            this.OverviewButtonHvacTempSetPoint.Text = "调整空调温度";
            this.OverviewButtonHvacTempSetPoint.UseVisualStyleBackColor = false;
            // 
            // OverviewButtonHumiditySetPoint
            // 
            this.OverviewButtonHumiditySetPoint.BackColor = System.Drawing.SystemColors.Control;
            this.OverviewButtonHumiditySetPoint.Location = new System.Drawing.Point(19, 282);
            this.OverviewButtonHumiditySetPoint.Name = "OverviewButtonHumiditySetPoint";
            this.OverviewButtonHumiditySetPoint.Size = new System.Drawing.Size(161, 30);
            this.OverviewButtonHumiditySetPoint.TabIndex = 5;
            this.OverviewButtonHumiditySetPoint.Text = "调整目标湿度";
            this.OverviewButtonHumiditySetPoint.UseVisualStyleBackColor = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.panel1);
            this.Name = "MainForm";
            this.Text = "注塑车间总控";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.OverviewPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel LogPanel;
        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.Panel OverviewPanel;
        private System.Windows.Forms.Label Overview;
        private System.Windows.Forms.Panel panel5;
        private moju.UI.StandardDoubleLabel LabelBoxWorkshopTemperature;
        private moju.UI.StandardDoubleLabel LabelBoxWorkshopHumidity;
        private moju.UI.StandardDoubleLabel LabelBoxWorkshopDewPoint;
        private moju.UI.StandardDoubleLabel LabelBoxWorkshopNoiseLevel;
        private System.Windows.Forms.Button OverviewButtonHvacTempSetPoint;
        private System.Windows.Forms.Button OverviewButtonHumiditySetPoint;
    }
}

