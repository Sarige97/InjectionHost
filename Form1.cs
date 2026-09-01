using moju.device.interfaces;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            TimerTaskManager.AddTimerEvent(1000, Initialize);
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

            ModbusSlave modbusSlave = new ModbusSlave("127.0.0.1", 502);
            Dictionary<ushort, bool> writableCoil = await modbusSlave.ReadWritableCoil(0x01, 0x00, 03);
            if(writableCoil != null)
            {
                Console.WriteLine("1:" + writableCoil[0].ToString() + "2:" + writableCoil[1].ToString() + "3:" + writableCoil[2].ToString());
            }
            
            //Dictionary<ushort, bool> readOnlyCoil = modbusSlave.ReadReadOnlyCoil(0x01, 0x00, 0x03);
            //Dictionary<ushort, ushort> writableRegister = modbusSlave.ReadWritableRegister(0x01, 0x00, 0x0A);
            //Dictionary<ushort, ushort> readOnlyRegister = modbusSlave.ReadReadOnlyRegister(0x01, 0x00, 0x0A);
            //modbusSlave.WriteSingleCoil(0x01, (ushort)10, true);
            //modbusSlave.WriteMultiCoil(0x01, 0, 16,  new bool[] {true, true, false, true, true, false, true, true, false, true, true, false, true, true, false, true});
            //modbusSlave.WriteSingleRegister(1, 0, 0x1234);
            //modbusSlave.WriteMultiRegister(1, 0, 16, new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF });
        }

        private void overviewBox_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
