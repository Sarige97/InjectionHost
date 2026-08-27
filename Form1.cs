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
        private void Initialize()
        {
            byte[] transactionBytes = TransactionIdGenerator.getTransactionIdBytesByIp("127.0.0.1", 502);
            Byte[] reqeustBytes = new Byte[12] { transactionBytes[0], transactionBytes[1], 0x00, 0x00, 0x00, 0x06, 0x01, 0x03, 0x00, 0x00, 0x00, 0x01 };

            byte[] bytes= ModbusTCPClientManager.modbusRequestByBytes("127.0.0.1", 502, reqeustBytes);
            Console.WriteLine(BitConverter.ToString(bytes));
        }

        private void overviewBox_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
