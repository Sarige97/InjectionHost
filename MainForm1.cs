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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace muju
{
    public partial class MainForm1 : Form
    {
        public MainForm1()
        {
            InitializeComponent();

            ModbusSlave injectionMoldingMachineSlave = SlaveManager.Instance.GetOrCreateSlave<InjectionMoldingMachineSlave>("127.0.0.1", 502, 1, "InjectionMoldingMachine", (ip, port, slaveId, catagoryName) => new InjectionMoldingMachineSlave(ip,port, slaveId, catagoryName));

            TimerTaskManager.AddTimerEvent(1000, Initialize);
            TimerTaskManager.AddTimerEvent(1000, injectionMoldingMachineSlave.RefreshData);
            TimerTaskManager.AddTimerEvent(1000, injectionMoldingMachineSlave.RefreshData);
        }

        public async void test()
        {
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
