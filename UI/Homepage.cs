using moju.config;
using moju.device;
using moju.device.interfaces;
using moju.domain;
using moju.log;
using moju.repository;
using moju.service;
using muju.modbus;
using muju.task;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using moju.tool;
using moju.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace muju
{
    public partial class Homepage : UserControl
    {
        public Homepage()
        {
            InitializeComponent();
            Initialize();
        }

        /// <summary>
        /// 初始化自定义组件
        /// </summary>
        private async void Initialize()
        {

            // 获取机器modbus映射和地址等配置数据
            List<SlaveDeviceInfo> IMInfoLIst = SlaveManager.GetModbusDeviceInfo("InjectionMoldingMachine");
            List<SlaveDeviceInfo> MTCInfoLIst = SlaveManager.GetModbusDeviceInfo("MoldTemperatureController");
            List<SlaveDeviceInfo> DRYInfoLIst = SlaveManager.GetModbusDeviceInfo("Dryer");
            List<SlaveDeviceInfo> ROBInfoLIst = SlaveManager.GetModbusDeviceInfo("Robot");
            List<SlaveDeviceInfo> PMInfoLIst = SlaveManager.GetModbusDeviceInfo("PowerMeter");
            List<SlaveDeviceInfo> WEInfoLIst = SlaveManager.GetModbusDeviceInfo("WorkshopEnvironment");

            // 注册注塑机的定时数据刷新
            foreach (SlaveDeviceInfo slaveDeviceInfo in IMInfoLIst)
            {
                ModbusSlaveTcp injectionMoldingMachineSlave = SlaveManager.GetOrCreateSlave<InjectionMoldingMachineSlave>(slaveDeviceInfo, (ip, port, slaveId, catagoryName) => new InjectionMoldingMachineSlave(ip, port, slaveId, catagoryName));
                TimerTaskManager.AddTimerEvent(1000, injectionMoldingMachineSlave.RefreshData);
            }

            // 注册模温机的定时数据刷新
            foreach (SlaveDeviceInfo slaveDeviceInfo in MTCInfoLIst)
            {
                ModbusSlaveTcp moldTemperatureControllerSlave = SlaveManager.GetOrCreateSlave<MoldTemperatureControllerSlave>(slaveDeviceInfo, (ip, port, slaveId, catagoryName) => new MoldTemperatureControllerSlave(ip, port, slaveId, catagoryName));
                TimerTaskManager.AddTimerEvent(1000, moldTemperatureControllerSlave.RefreshData);
            }

            // 注册干燥机的定时数据刷新
            foreach (SlaveDeviceInfo slaveDeviceInfo in DRYInfoLIst)
            {
                ModbusSlaveTcp drySlave = SlaveManager.GetOrCreateSlave<DryerSlave>(slaveDeviceInfo, (ip, port, slaveId, catagoryName) => new DryerSlave(ip, port, slaveId, catagoryName));
                TimerTaskManager.AddTimerEvent(1000, drySlave.RefreshData);
            }

            // 注册机械臂的定时数据刷新
            foreach (SlaveDeviceInfo slaveDeviceInfo in ROBInfoLIst)
            {
                ModbusSlaveTcp robotSlave = SlaveManager.GetOrCreateSlave<RobotSlave>(slaveDeviceInfo, (ip, port, slaveId, catagoryName) => new RobotSlave(ip, port, slaveId, catagoryName));
                TimerTaskManager.AddTimerEvent(1000, robotSlave.RefreshData);
            }

            // 注册电表的定时数据刷新
            foreach (SlaveDeviceInfo slaveDeviceInfo in PMInfoLIst)
            {
                ModbusSlaveTcp powerMeterSlave = SlaveManager.GetOrCreateSlave<PowerMeterSlave>(slaveDeviceInfo, (ip, port, slaveId, catagoryName) => new PowerMeterSlave(ip, port, slaveId, catagoryName));
                TimerTaskManager.AddTimerEvent(1000, powerMeterSlave.RefreshData);
            }

            // 注册车间环境的定时数据刷新
            foreach (SlaveDeviceInfo slaveDeviceInfo in WEInfoLIst)
            {
                ModbusSlaveTcp workshopEnvironmentSlave = SlaveManager.GetOrCreateSlave<WorkshopEnvironmentSlave>(slaveDeviceInfo, (ip, port, slaveId, catagoryName) => new WorkshopEnvironmentSlave(ip, port, slaveId, catagoryName));
                TimerTaskManager.AddTimerEvent(1000, workshopEnvironmentSlave.RefreshData);
            }

            TimerTaskManager.AddTimerEvent(500, this.refreshWorkshopEnvironmentData);

            List<InjectMachineGroup> InjectMachineGroupList = new List<InjectMachineGroup>();
            InjectMachineGroupList.Add(injectMachineGroup1);
            InjectMachineGroupList.Add(injectMachineGroup2);
            InjectMachineGroupList.Add(injectMachineGroup3);
            InjectMachineGroupList.Add(injectMachineGroup4);

            // 初始化自定义注塑机组控件
            // 订阅注塑机概览栏的详情按钮点击事件

            foreach (InjectMachineGroup injectMachineGroup in InjectMachineGroupList)
            {
                injectMachineGroup.Init();
                TimerTaskManager.AddTimerEvent(500, injectMachineGroup.RefreshData);
                injectMachineGroup.InjectionMachineGroupDetailButtonClick += ChangeMainPanel2Detail;
            }



            //this.injectMachineGroup1.Init();
            //this.injectMachineGroup2.Init();
            //this.injectMachineGroup3.Init();
            //this.injectMachineGroup4.Init();
            //TimerTaskManager.AddTimerEvent(500, this.injectMachineGroup1.RefreshData);
            //TimerTaskManager.AddTimerEvent(500, this.injectMachineGroup2.RefreshData);
            //TimerTaskManager.AddTimerEvent(500, this.injectMachineGroup3.RefreshData);
            //TimerTaskManager.AddTimerEvent(500, this.injectMachineGroup4.RefreshData);




        }

        public async void refreshWorkshopEnvironmentData()
        {
            List<SlaveDeviceInfo> WEInfoLIst = SlaveManager.GetModbusDeviceInfo("WorkshopEnvironment");
            List<SlaveDeviceInfo> PWInfoList = SlaveManager.GetModbusDeviceInfo("PowerMeter");
            // 获取总览 - 车间温度
            WorkshopEnvironmentSlave workshopEnvironmentSlave = SlaveManager.GetOrCreateSlave<WorkshopEnvironmentSlave>(WEInfoLIst[0], null);
            PowerMeterSlave powerMeterSlave = SlaveManager.GetOrCreateSlave<PowerMeterSlave>(PWInfoList[0], null);
            LabelBoxWorkshopTemperature.RightLabelText = workshopEnvironmentSlave.WorkshopTemp;
            LabelBoxWorkshopHumidity.RightLabelText = workshopEnvironmentSlave.Humidity;
            LabelBoxWorkshopDewPoint.RightLabelText = workshopEnvironmentSlave.DewPoint;
            LabelBoxWorkshopNoiseLevel.RightLabelText = workshopEnvironmentSlave.NoiseLevel;
            LabelBoxWorkshopTargetTemperature.RightLabelText = workshopEnvironmentSlave.HvacTempSetPoint;
            LabelBoxWorkshopTargetHumidity.RightLabelText = workshopEnvironmentSlave.HumiditySetPoint;
            LabelBoxPower.RightLabelText = powerMeterSlave.TotalEnergy;
            // 获取设备的高温报警信号
            if (workshopEnvironmentSlave.HighTempAlarm)
            {
                LabelBoxWorkshopTemperature.ForeColor = Color.Red;
            }
            else
            {
                LabelBoxWorkshopTemperature.ForeColor = SystemColors.GrayText;
            }
        }

        public void ChangeMainPanel2Detail(object sender, InjectMachineGroup injectMachineGroup)
        {
            // 清空主页空间
            MainPanel.Controls.Clear();
            InjectionGroupDetail detail = new InjectionGroupDetail(injectMachineGroup.InjectMachineId, injectMachineGroup.MoldTempMachineId, injectMachineGroup.DryerId, injectMachineGroup.RobotId);
            // 先订阅详情页面的返回事件
            detail.InjectionMachineGroupDetailBackButtonClick += ChangeMainPanel2Overview;
            MainPanel.Controls.Add(detail);
            TimerTaskManager.AddTimerEvent(1000, detail.RefreshData);
        }

        public void ChangeMainPanel2Overview(object sender, EventArgs args)
        {
            MainPanel.Controls.Clear();
            MainPanel.Controls.Add(flowLayoutPanel);
            if (sender is InjectMachineGroup)
            {
                // 如果是从注塑机组回到主页，则销毁原来的控件
                ((InjectMachineGroup)sender).Dispose();
            }
        }

        private async void OverviewButtonHvacTempSetPoint_Click(object sender, EventArgs e)
        {
            string temp = Interaction.InputBox("请输入目标温度，例如：13.4  15  -3.5", "温度设置", "", -1, -1);
            List<SlaveDeviceInfo> WEInfoLIst = SlaveManager.GetModbusDeviceInfo("WorkshopEnvironment");
            WorkshopEnvironmentSlave workshopEnvironmentSlave = SlaveManager.GetOrCreateSlave<WorkshopEnvironmentSlave>(WEInfoLIst[0], null);
            try
            {
                bool setResult = await workshopEnvironmentSlave.setHvacTEmpSetPoint(temp);
                if (setResult)
                {
                    MessageBox.Show("设定成功", "确认", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("设定失败，发送指令时遇到错误", "确认", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show("设定失败," + exception.Message, "确认", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private async void OverviewButtonHumiditySetPoint_Click(object sender, EventArgs e)
        {
            string temp = Interaction.InputBox("请输入目标湿度，0-100的整数", "湿度设置", "", -1, -1);
            List<SlaveDeviceInfo> WEInfoLIst = SlaveManager.GetModbusDeviceInfo("WorkshopEnvironment");
            WorkshopEnvironmentSlave workshopEnvironmentSlave = SlaveManager.GetOrCreateSlave<WorkshopEnvironmentSlave>(WEInfoLIst[0], null);
            try
            {
                bool setResult = await workshopEnvironmentSlave.setHumiditySetPoint(temp);
                if (setResult)
                {
                    MessageBox.Show("设定成功", "确认", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("设定成功，发送指令时遇到错误", "确认", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show("设定成功," + exception.Message, "确认", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
