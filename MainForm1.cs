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
    public partial class MainForm1 : Form
    {
        public MainForm1()
        {
            InitializeComponent();
            //ModbusSlaveTcp injectionMoldingMachineSlave = SlaveManager.GetOrCreateSlave<InjectionMoldingMachineSlave>("127.0.0.1", 502, 1, "InjectionMoldingMachine", (ip, port, slaveId, catagoryName) => new InjectionMoldingMachineSlave(ip, port, slaveId, catagoryName));
            //ModbusSlaveTcp moldTemperatureControllerSlave = SlaveManager.GetOrCreateSlave<MoldTemperatureControllerSlave>("127.0.0.1", 503, 2, "MoldTemperatureController", (ip, port, slaveId, catagoryName) => new MoldTemperatureControllerSlave(ip, port, slaveId, catagoryName));
            //ModbusSlaveTcp dryerSlave = SlaveManager.GetOrCreateSlave<DryerSlave>("127.0.0.1", 504, 3, "Dryer", (ip, port, slaveId, catagoryName) => new DryerSlave(ip, port, slaveId, catagoryName));
            //ModbusSlaveTcp robotSlave = SlaveManager.GetOrCreateSlave<RobotSlave>("127.0.0.1", 505, 4, "Robot", (ip, port, slaveId, catagoryName) => new RobotSlave(ip, port, slaveId, catagoryName));

            //ModbusSlaveTcp powerMeterSlave = SlaveManager.GetOrCreateSlave<PowerMeterSlave>("127.0.0.1", 518, 61, "PowerMeter", (ip, port, slaveId, catagoryName) => new PowerMeterSlave(ip, port, slaveId, catagoryName));
            //ModbusSlaveTcp workshopEnvironmentSlave = SlaveManager.GetOrCreateSlave<WorkshopEnvironmentSlave>("127.0.0.1", 519, 71, "WorkshopEnvironment", (ip, port, slaveId, catagoryName) => new WorkshopEnvironmentSlave(ip, port, slaveId, catagoryName));

            // 获取机器modbus映射和地址等配置数据
            List<SlaveDeviceInfo> IMInfoLIst = SlaveManager.getModbusSlaveInstance("InjectionMoldingMachine");
            List<SlaveDeviceInfo> MTCInfoLIst = SlaveManager.getModbusSlaveInstance("MoldTemperatureController");
            List<SlaveDeviceInfo> DRYInfoLIst = SlaveManager.getModbusSlaveInstance("Dryer");
            List<SlaveDeviceInfo> ROBInfoLIst = SlaveManager.getModbusSlaveInstance("Robot");
            List<SlaveDeviceInfo> PMInfoLIst = SlaveManager.getModbusSlaveInstance("PowerMeter");
            List<SlaveDeviceInfo> WEInfoLIst = SlaveManager.getModbusSlaveInstance("WorkshopEnvironment");

            // 注册注塑机的定时数据刷新
            foreach (SlaveDeviceInfo slaveDeviceInfo in IMInfoLIst)
            {
                ModbusSlaveTcp injectionMoldingMachineSlave = SlaveManager.GetOrCreateSlave<InjectionMoldingMachineSlave>(slaveDeviceInfo.Ip, slaveDeviceInfo.Port, slaveDeviceInfo.SlaveId, slaveDeviceInfo.SlaveCatagoryName, (ip, port, slaveId, catagoryName) => new InjectionMoldingMachineSlave(ip, port, slaveId, catagoryName));
                TimerTaskManager.AddTimerEvent(1000, injectionMoldingMachineSlave.RefreshData);
            }

            // 注册模温机的定时数据刷新
            foreach (SlaveDeviceInfo slaveDeviceInfo in MTCInfoLIst)
            {
                ModbusSlaveTcp moldTemperatureControllerSlave = SlaveManager.GetOrCreateSlave<MoldTemperatureControllerSlave>(slaveDeviceInfo.Ip, slaveDeviceInfo.Port, slaveDeviceInfo.SlaveId, slaveDeviceInfo.SlaveCatagoryName, (ip, port, slaveId, catagoryName) => new MoldTemperatureControllerSlave(ip, port, slaveId, catagoryName));
                TimerTaskManager.AddTimerEvent(1000, moldTemperatureControllerSlave.RefreshData);
            }

            // 注册干燥机的定时数据刷新
            foreach (SlaveDeviceInfo slaveDeviceInfo in DRYInfoLIst)
            {
                ModbusSlaveTcp drySlave = SlaveManager.GetOrCreateSlave<DryerSlave>(slaveDeviceInfo.Ip, slaveDeviceInfo.Port, slaveDeviceInfo.SlaveId, slaveDeviceInfo.SlaveCatagoryName, (ip, port, slaveId, catagoryName) => new DryerSlave(ip, port, slaveId, catagoryName));
                TimerTaskManager.AddTimerEvent(1000, drySlave.RefreshData);
            }

            // 注册机械臂的定时数据刷新
            foreach (SlaveDeviceInfo slaveDeviceInfo in ROBInfoLIst)
            {
                ModbusSlaveTcp robotSlave = SlaveManager.GetOrCreateSlave<RobotSlave>(slaveDeviceInfo.Ip, slaveDeviceInfo.Port, slaveDeviceInfo.SlaveId, slaveDeviceInfo.SlaveCatagoryName, (ip, port, slaveId, catagoryName) => new RobotSlave(ip, port, slaveId, catagoryName));
                TimerTaskManager.AddTimerEvent(1000, robotSlave.RefreshData);
            }

            // 注册电表的定时数据刷新
            foreach (SlaveDeviceInfo slaveDeviceInfo in PMInfoLIst)
            {
                ModbusSlaveTcp powerMeterSlave = SlaveManager.GetOrCreateSlave<PowerMeterSlave>(slaveDeviceInfo.Ip, slaveDeviceInfo.Port, slaveDeviceInfo.SlaveId, slaveDeviceInfo.SlaveCatagoryName, (ip, port, slaveId, catagoryName) => new PowerMeterSlave(ip, port, slaveId, catagoryName));
                TimerTaskManager.AddTimerEvent(1000, powerMeterSlave.RefreshData);
            }

            // 注册车间环境的定时数据刷新
            foreach (SlaveDeviceInfo slaveDeviceInfo in WEInfoLIst)
            {
                ModbusSlaveTcp workshopEnvironmentSlave = SlaveManager.GetOrCreateSlave<WorkshopEnvironmentSlave>(slaveDeviceInfo.Ip, slaveDeviceInfo.Port, slaveDeviceInfo.SlaveId, slaveDeviceInfo.SlaveCatagoryName, (ip, port, slaveId, catagoryName) => new WorkshopEnvironmentSlave(ip, port, slaveId, catagoryName));
                TimerTaskManager.AddTimerEvent(1000, workshopEnvironmentSlave.RefreshData);
            }



            TimerTaskManager.AddTimerEvent(1000, Initialize);
            TimerTaskManager.AddTimerEvent(1000, TestLoop);

            Test();
        }

        public void Test()
        {

        }

        public async void TestLoop()
        {

            InjectionMoldingMachineSlave injectionMoldingMachineSlave1 = SlaveManager.GetOrCreateSlave<InjectionMoldingMachineSlave>("127.0.0.1", 502, 1, "InjectionMoldingMachine", null);
            MoldTemperatureControllerSlave moldTemperatureControllerSlave1 = SlaveManager.GetOrCreateSlave<MoldTemperatureControllerSlave>("127.0.0.1", 503, 2, "MoldTemperatureController", null);
            DryerSlave dryerSlave = SlaveManager.GetOrCreateSlave<DryerSlave>("127.0.0.1", 504, 3, "Dryer", null);
            RobotSlave robotSlave = SlaveManager.GetOrCreateSlave<RobotSlave>("127.0.0.1", 505, 4, "Robot", null);
            PowerMeterSlave powerMeterSlave = SlaveManager.GetOrCreateSlave<PowerMeterSlave>("127.0.0.1", 518, 61, "PowerMeter", (ip, port, slaveId, catagoryName) => new PowerMeterSlave(ip, port, slaveId, catagoryName));
            WorkshopEnvironmentSlave workshopEnvironmentSlave = SlaveManager.GetOrCreateSlave<WorkshopEnvironmentSlave>("127.0.0.1", 519, 71, "WorkshopEnvironment", (ip, port, slaveId, catagoryName) => new WorkshopEnvironmentSlave(ip, port, slaveId, catagoryName));

            SimpleLogger.Instance.Info("模温机数据:" + moldTemperatureControllerSlave1.PumpFlow.ToString("D"));
            SimpleLogger.Instance.Info("注塑机数据:" + injectionMoldingMachineSlave1.BarrelTemp1.ToString("D"));
            SimpleLogger.Instance.Info("干燥剂数据:" + dryerSlave.HeatingPower.ToString("D"));
            SimpleLogger.Instance.Info("机械臂数据:" + robotSlave.TotalPicks.ToString("D"));
            SimpleLogger.Instance.Info("电能器数据:" + powerMeterSlave.TotalEnergy.ToString("D"));
            SimpleLogger.Instance.Info("机械间数据:" + workshopEnvironmentSlave.Humidity.ToString("D"));
        }

        /// <summary>
        /// 初始化自定义组件
        /// </summary>
        private async void Initialize()
        {

            //byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp("127.0.0.1", 502);
            //Byte[] reqeustBytes = new Byte[12] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, 0x00, 0x00, 0x00, 0x01 };

            //byte[] bytes= ModbusTCPClientManager.modbusRequestByBytes("127.0.0.1", 502, reqeustBytes);
            //Console.WriteLine(BitConverter.ToString(bytes));


            //Dictionary<ushort, bool> readOnlyCoil = modbusSlave.ReadReadOnlyCoil(0x01, 0x00, 0x03);
            //Dictionary<ushort, ushort> writableRegister = modbusSlave.ReadWritableRegister(0x01, 0x00, 0x0A);
            //Dictionary<ushort, ushort> readOnlyRegister = modbusSlave.ReadReadOnlyRegister(0x01, 0x00, 0x0A);
            //modbusSlave.WriteSingleCoil(0x01, (ushort)10, true);
            //modbusSlave.WriteMultiCoil(0x01, 0, 16,  new bool[] {true, true, false, true, true, false, true, true, false, true, true, false, true, true, false, true});
            //modbusSlave.WriteSingleRegister(1, 0, 0x1234);
            //modbusSlave.WriteMultiRegister(1, 0, 16, new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF });
            //ConfigManager instance = ConfigManager.Instance;

            //SimpleLogger.Instance.Debug("DebugTest");
            //SimpleLogger.Instance.Info("InfoTest");
            //SimpleLogger.Instance.Warn("WarnTest");
            //SimpleLogger.Instance.Error("ErrorTest");
            //SimpleLogger.Instance.Fatal("FatalTest");


        }

        private void overviewBox_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
