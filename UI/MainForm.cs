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

namespace muju
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Initialize();
        }

        public void Test()
        {

        }

        public async void TestLoop()
        {

            //InjectionMoldingMachineSlave injectionMoldingMachineSlave1 = SlaveManager.GetOrCreateSlave<InjectionMoldingMachineSlave>("127.0.0.1", 502, 1, "InjectionMoldingMachine", null);
            //MoldTemperatureControllerSlave moldTemperatureControllerSlave1 = SlaveManager.GetOrCreateSlave<MoldTemperatureControllerSlave>("127.0.0.1", 503, 2, "MoldTemperatureController", null);
            //DryerSlave dryerSlave = SlaveManager.GetOrCreateSlave<DryerSlave>("127.0.0.1", 504, 3, "Dryer", null);
            //RobotSlave robotSlave = SlaveManager.GetOrCreateSlave<RobotSlave>("127.0.0.1", 505, 4, "Robot", null);
            //PowerMeterSlave powerMeterSlave = SlaveManager.GetOrCreateSlave<PowerMeterSlave>("127.0.0.1", 518, 61, "PowerMeter", (ip, port, slaveId, catagoryName) => new PowerMeterSlave(ip, port, slaveId, catagoryName));
            //WorkshopEnvironmentSlave workshopEnvironmentSlave = SlaveManager.GetOrCreateSlave<WorkshopEnvironmentSlave>("127.0.0.1", 519, 71, "WorkshopEnvironment", (ip, port, slaveId, catagoryName) => new WorkshopEnvironmentSlave(ip, port, slaveId, catagoryName));

            //SimpleLogger.Instance.Info("模温机数据:" + moldTemperatureControllerSlave1.PumpFlow.ToString("D"));
            //SimpleLogger.Instance.Info("模温机数据:" + moldTemperatureControllerSlave1.Running);
            //SimpleLogger.Instance.Info("注塑机数据:" + injectionMoldingMachineSlave1.BarrelTemp1.ToString("D"));
            //SimpleLogger.Instance.Info("干燥剂数据:" + dryerSlave.HeatingPower.ToString("D"));
            //SimpleLogger.Instance.Info("机械臂数据:" + robotSlave.TotalPicks.ToString("D"));
            //SimpleLogger.Instance.Info("电能器数据:" + powerMeterSlave.TotalEnergy.ToString("D"));
            //SimpleLogger.Instance.Info("机械间数据:" + workshopEnvironmentSlave.Humidity.ToString("D"));
        }

        /// <summary>
        /// 初始化自定义组件
        /// </summary>
        private async void Initialize()
        {

            // 获取机器modbus映射和地址等配置数据
            List<SlaveDeviceInfo> IMInfoLIst = SlaveManager.getModbusDeviceInfo("InjectionMoldingMachine");
            List<SlaveDeviceInfo> MTCInfoLIst = SlaveManager.getModbusDeviceInfo("MoldTemperatureController");
            List<SlaveDeviceInfo> DRYInfoLIst = SlaveManager.getModbusDeviceInfo("Dryer");
            List<SlaveDeviceInfo> ROBInfoLIst = SlaveManager.getModbusDeviceInfo("Robot");
            List<SlaveDeviceInfo> PMInfoLIst = SlaveManager.getModbusDeviceInfo("PowerMeter");
            List<SlaveDeviceInfo> WEInfoLIst = SlaveManager.getModbusDeviceInfo("WorkshopEnvironment");

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

            TimerTaskManager.AddTimerEvent(100, this.refreshWorkshopEnvironmentData);


        }

        private void overviewBox_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public async void refreshWorkshopEnvironmentData()
        {
            List<SlaveDeviceInfo> WEInfoLIst = SlaveManager.getModbusDeviceInfo("WorkshopEnvironment");
            // 获取总览 - 车间温度
            WorkshopEnvironmentSlave workshopEnvironmentSlave = SlaveManager.GetOrCreateSlave<WorkshopEnvironmentSlave>(WEInfoLIst[0], null);
            LabelBoxWorkshopTemperature.RightLabelText = workshopEnvironmentSlave.WorkshopTemp;
            LabelBoxWorkshopHumidity.RightLabelText = workshopEnvironmentSlave.Humidity;
            LabelBoxWorkshopDewPoint.RightLabelText = workshopEnvironmentSlave.DewPoint;
            LabelBoxWorkshopNoiseLevel.RightLabelText = workshopEnvironmentSlave.NoiseLevel;
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
    }
}
