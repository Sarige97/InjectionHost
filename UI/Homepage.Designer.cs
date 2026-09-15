namespace muju
{
    partial class Homepage
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
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.injectMachineGroup1 = new moju.UI.InjectMachineGroup();
            this.injectMachineGroup2 = new moju.UI.InjectMachineGroup();
            this.injectMachineGroup3 = new moju.UI.InjectMachineGroup();
            this.injectMachineGroup4 = new moju.UI.InjectMachineGroup();
            this.OverviewPanel = new System.Windows.Forms.Panel();
            this.LabelBoxPower = new moju.UI.StandardDoubleLabel();
            this.LabelBoxWorkshopTargetHumidity = new moju.UI.StandardDoubleLabel();
            this.LabelBoxWorkshopTargetTemperature = new moju.UI.StandardDoubleLabel();
            this.OverviewButtonHumiditySetPoint = new System.Windows.Forms.Button();
            this.OverviewButtonHvacTempSetPoint = new System.Windows.Forms.Button();
            this.LabelBoxWorkshopNoiseLevel = new moju.UI.StandardDoubleLabel();
            this.LabelBoxWorkshopDewPoint = new moju.UI.StandardDoubleLabel();
            this.LabelBoxWorkshopHumidity = new moju.UI.StandardDoubleLabel();
            this.LabelBoxWorkshopTemperature = new moju.UI.StandardDoubleLabel();
            this.LogRichText = new System.Windows.Forms.RichTextBox();
            this.panel1.SuspendLayout();
            this.LogPanel.SuspendLayout();
            this.panel5.SuspendLayout();
            this.MainPanel.SuspendLayout();
            this.flowLayoutPanel.SuspendLayout();
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
            this.panel1.Size = new System.Drawing.Size(1918, 1200);
            this.panel1.TabIndex = 0;
            // 
            // LogPanel
            // 
            this.LogPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.LogPanel.Controls.Add(this.LogRichText);
            this.LogPanel.Location = new System.Drawing.Point(221, 855);
            this.LogPanel.Name = "LogPanel";
            this.LogPanel.Size = new System.Drawing.Size(1693, 215);
            this.LogPanel.TabIndex = 0;
            // 
            // panel5
            // 
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel5.Controls.Add(this.Overview);
            this.panel5.Location = new System.Drawing.Point(3, 10);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(211, 105);
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
            this.MainPanel.Controls.Add(this.flowLayoutPanel);
            this.MainPanel.Location = new System.Drawing.Point(220, 10);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(1693, 839);
            this.MainPanel.TabIndex = 0;
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.AutoScroll = true;
            this.flowLayoutPanel.Controls.Add(this.injectMachineGroup1);
            this.flowLayoutPanel.Controls.Add(this.injectMachineGroup2);
            this.flowLayoutPanel.Controls.Add(this.injectMachineGroup3);
            this.flowLayoutPanel.Controls.Add(this.injectMachineGroup4);
            this.flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(1691, 837);
            this.flowLayoutPanel.TabIndex = 0;
            this.flowLayoutPanel.WrapContents = false;
            // 
            // injectMachineGroup1
            // 
            this.injectMachineGroup1.BackColor = System.Drawing.SystemColors.Control;
            this.injectMachineGroup1.DryerId = "DRY-01";
            this.injectMachineGroup1.InjectMachineId = "IM-01";
            this.injectMachineGroup1.Location = new System.Drawing.Point(3, 3);
            this.injectMachineGroup1.MoldTempMachineId = "MTC-01";
            this.injectMachineGroup1.Name = "injectMachineGroup1";
            this.injectMachineGroup1.RobotId = "ROB-01";
            this.injectMachineGroup1.Size = new System.Drawing.Size(1650, 300);
            this.injectMachineGroup1.TabIndex = 2;
            // 
            // injectMachineGroup2
            // 
            this.injectMachineGroup2.BackColor = System.Drawing.SystemColors.Control;
            this.injectMachineGroup2.DryerId = "DRY-02";
            this.injectMachineGroup2.InjectMachineId = "IM-02";
            this.injectMachineGroup2.Location = new System.Drawing.Point(3, 309);
            this.injectMachineGroup2.MoldTempMachineId = "MTC-02";
            this.injectMachineGroup2.Name = "injectMachineGroup2";
            this.injectMachineGroup2.RobotId = "ROB-02";
            this.injectMachineGroup2.Size = new System.Drawing.Size(1650, 300);
            this.injectMachineGroup2.TabIndex = 3;
            // 
            // injectMachineGroup3
            // 
            this.injectMachineGroup3.BackColor = System.Drawing.SystemColors.Control;
            this.injectMachineGroup3.DryerId = "DRY-03";
            this.injectMachineGroup3.InjectMachineId = "IM-03";
            this.injectMachineGroup3.Location = new System.Drawing.Point(3, 615);
            this.injectMachineGroup3.MoldTempMachineId = "MTC-03";
            this.injectMachineGroup3.Name = "injectMachineGroup3";
            this.injectMachineGroup3.RobotId = "ROB-03";
            this.injectMachineGroup3.Size = new System.Drawing.Size(1650, 300);
            this.injectMachineGroup3.TabIndex = 4;
            // 
            // injectMachineGroup4
            // 
            this.injectMachineGroup4.BackColor = System.Drawing.SystemColors.Control;
            this.injectMachineGroup4.DryerId = "DRY-04";
            this.injectMachineGroup4.InjectMachineId = "IM-04";
            this.injectMachineGroup4.Location = new System.Drawing.Point(3, 921);
            this.injectMachineGroup4.MoldTempMachineId = "MTC-04";
            this.injectMachineGroup4.Name = "injectMachineGroup4";
            this.injectMachineGroup4.RobotId = "ROB-04";
            this.injectMachineGroup4.Size = new System.Drawing.Size(1650, 300);
            this.injectMachineGroup4.TabIndex = 5;
            // 
            // OverviewPanel
            // 
            this.OverviewPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OverviewPanel.Controls.Add(this.LabelBoxPower);
            this.OverviewPanel.Controls.Add(this.LabelBoxWorkshopTargetHumidity);
            this.OverviewPanel.Controls.Add(this.LabelBoxWorkshopTargetTemperature);
            this.OverviewPanel.Controls.Add(this.OverviewButtonHumiditySetPoint);
            this.OverviewPanel.Controls.Add(this.OverviewButtonHvacTempSetPoint);
            this.OverviewPanel.Controls.Add(this.LabelBoxWorkshopNoiseLevel);
            this.OverviewPanel.Controls.Add(this.LabelBoxWorkshopDewPoint);
            this.OverviewPanel.Controls.Add(this.LabelBoxWorkshopHumidity);
            this.OverviewPanel.Controls.Add(this.LabelBoxWorkshopTemperature);
            this.OverviewPanel.Location = new System.Drawing.Point(3, 10);
            this.OverviewPanel.Name = "OverviewPanel";
            this.OverviewPanel.Size = new System.Drawing.Size(211, 1064);
            this.OverviewPanel.TabIndex = 0;
            // 
            // LabelBoxPower
            // 
            this.LabelBoxPower.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LabelBoxPower.LeftLabelText = "总电量";
            this.LabelBoxPower.Location = new System.Drawing.Point(-108, 1031);
            this.LabelBoxPower.Name = "LabelBoxPower";
            this.LabelBoxPower.RightLabelText = "0kwh";
            this.LabelBoxPower.Size = new System.Drawing.Size(303, 28);
            this.LabelBoxPower.TabIndex = 8;
            // 
            // LabelBoxWorkshopTargetHumidity
            // 
            this.LabelBoxWorkshopTargetHumidity.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LabelBoxWorkshopTargetHumidity.LeftLabelText = "目标温度";
            this.LabelBoxWorkshopTargetHumidity.Location = new System.Drawing.Point(-83, 212);
            this.LabelBoxWorkshopTargetHumidity.Name = "LabelBoxWorkshopTargetHumidity";
            this.LabelBoxWorkshopTargetHumidity.RightLabelText = "0℃";
            this.LabelBoxWorkshopTargetHumidity.Size = new System.Drawing.Size(303, 28);
            this.LabelBoxWorkshopTargetHumidity.TabIndex = 7;
            // 
            // LabelBoxWorkshopTargetTemperature
            // 
            this.LabelBoxWorkshopTargetTemperature.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LabelBoxWorkshopTargetTemperature.LeftLabelText = "目标温度";
            this.LabelBoxWorkshopTargetTemperature.Location = new System.Drawing.Point(-83, 144);
            this.LabelBoxWorkshopTargetTemperature.Name = "LabelBoxWorkshopTargetTemperature";
            this.LabelBoxWorkshopTargetTemperature.RightLabelText = "0℃";
            this.LabelBoxWorkshopTargetTemperature.Size = new System.Drawing.Size(303, 28);
            this.LabelBoxWorkshopTargetTemperature.TabIndex = 6;
            // 
            // OverviewButtonHumiditySetPoint
            // 
            this.OverviewButtonHumiditySetPoint.BackColor = System.Drawing.SystemColors.Control;
            this.OverviewButtonHumiditySetPoint.Location = new System.Drawing.Point(19, 350);
            this.OverviewButtonHumiditySetPoint.Name = "OverviewButtonHumiditySetPoint";
            this.OverviewButtonHumiditySetPoint.Size = new System.Drawing.Size(161, 30);
            this.OverviewButtonHumiditySetPoint.TabIndex = 5;
            this.OverviewButtonHumiditySetPoint.Text = "调整目标湿度";
            this.OverviewButtonHumiditySetPoint.UseVisualStyleBackColor = false;
            this.OverviewButtonHumiditySetPoint.Click += new System.EventHandler(this.OverviewButtonHumiditySetPoint_Click);
            // 
            // OverviewButtonHvacTempSetPoint
            // 
            this.OverviewButtonHvacTempSetPoint.BackColor = System.Drawing.SystemColors.Control;
            this.OverviewButtonHvacTempSetPoint.Location = new System.Drawing.Point(19, 314);
            this.OverviewButtonHvacTempSetPoint.Name = "OverviewButtonHvacTempSetPoint";
            this.OverviewButtonHvacTempSetPoint.Size = new System.Drawing.Size(161, 30);
            this.OverviewButtonHvacTempSetPoint.TabIndex = 4;
            this.OverviewButtonHvacTempSetPoint.Text = "调整空调温度";
            this.OverviewButtonHvacTempSetPoint.UseVisualStyleBackColor = false;
            this.OverviewButtonHvacTempSetPoint.Click += new System.EventHandler(this.OverviewButtonHvacTempSetPoint_Click);
            // 
            // LabelBoxWorkshopNoiseLevel
            // 
            this.LabelBoxWorkshopNoiseLevel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LabelBoxWorkshopNoiseLevel.LeftLabelText = "噪音";
            this.LabelBoxWorkshopNoiseLevel.Location = new System.Drawing.Point(-83, 280);
            this.LabelBoxWorkshopNoiseLevel.Name = "LabelBoxWorkshopNoiseLevel";
            this.LabelBoxWorkshopNoiseLevel.RightLabelText = "0℃";
            this.LabelBoxWorkshopNoiseLevel.Size = new System.Drawing.Size(303, 28);
            this.LabelBoxWorkshopNoiseLevel.TabIndex = 3;
            // 
            // LabelBoxWorkshopDewPoint
            // 
            this.LabelBoxWorkshopDewPoint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LabelBoxWorkshopDewPoint.LeftLabelText = "露点";
            this.LabelBoxWorkshopDewPoint.Location = new System.Drawing.Point(-83, 246);
            this.LabelBoxWorkshopDewPoint.Name = "LabelBoxWorkshopDewPoint";
            this.LabelBoxWorkshopDewPoint.RightLabelText = "0℃";
            this.LabelBoxWorkshopDewPoint.Size = new System.Drawing.Size(303, 28);
            this.LabelBoxWorkshopDewPoint.TabIndex = 2;
            // 
            // LabelBoxWorkshopHumidity
            // 
            this.LabelBoxWorkshopHumidity.ForeColor = System.Drawing.SystemColors.GrayText;
            this.LabelBoxWorkshopHumidity.LeftLabelText = "车间湿度";
            this.LabelBoxWorkshopHumidity.Location = new System.Drawing.Point(-83, 178);
            this.LabelBoxWorkshopHumidity.Name = "LabelBoxWorkshopHumidity";
            this.LabelBoxWorkshopHumidity.RightLabelText = "0%";
            this.LabelBoxWorkshopHumidity.Size = new System.Drawing.Size(303, 28);
            this.LabelBoxWorkshopHumidity.TabIndex = 1;
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
            // LogRichText
            // 
            this.LogRichText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogRichText.Location = new System.Drawing.Point(0, 0);
            this.LogRichText.Name = "LogRichText";
            this.LogRichText.Size = new System.Drawing.Size(1691, 213);
            this.LogRichText.TabIndex = 0;
            this.LogRichText.Text = "";
            // 
            // Homepage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "Homepage";
            this.Size = new System.Drawing.Size(1920, 1080);
            this.panel1.ResumeLayout(false);
            this.LogPanel.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.flowLayoutPanel.ResumeLayout(false);
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
        private moju.UI.StandardDoubleLabel LabelBoxWorkshopTargetHumidity;
        private moju.UI.StandardDoubleLabel LabelBoxWorkshopTargetTemperature;
        private moju.UI.StandardDoubleLabel LabelBoxPower;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private moju.UI.InjectMachineGroup injectMachineGroup1;
        private moju.UI.InjectMachineGroup injectMachineGroup2;
        private moju.UI.InjectMachineGroup injectMachineGroup3;
        private moju.UI.InjectMachineGroup injectMachineGroup4;
        private System.Windows.Forms.RichTextBox LogRichText;
    }
}

