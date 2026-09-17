using moju.device;
using moju.device.interfaces;
using moju.domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace moju.UI
{
    public partial class SetBarrelTemp : Form

    {
        public short BarrelTemp1 { get; set; }
        public short BarrelTemp2 { get; set; }
        public short BarrelTemp3 { get; set; }
        public short BarrelTemp4 { get; set; }
        public short BarrelTemp5 { get; set; }

        private string regex = @"\d+\.\d";


        public SetBarrelTemp(ModbusSlaveTcp injectSlave)
        {
            InitializeComponent();
            if (injectSlave is InjectionMoldingMachineSlave)
            {
                Input_BarrelTemp1.Text = changeText2Dot1(((InjectionMoldingMachineSlave)injectSlave).BarrelSetpoint1);
                Input_BarrelTemp2.Text = changeText2Dot1(((InjectionMoldingMachineSlave)injectSlave).BarrelSetpoint2);
                Input_BarrelTemp3.Text = changeText2Dot1(((InjectionMoldingMachineSlave)injectSlave).BarrelSetpoint3);
                Input_BarrelTemp4.Text = changeText2Dot1(((InjectionMoldingMachineSlave)injectSlave).BarrelSetpoint4);
                Input_BarrelTemp5.Text = changeText2Dot1(((InjectionMoldingMachineSlave)injectSlave).BarrelSetpoint5);
            }

        }

        private string changeText2Dot1(string text)
        {
            if (int.TryParse(text, out int result))
            {
                return Convert.ToString(result / 10.0F);
            }
            else
            {
                return text;
            }
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void Btn_OK_Click(object sender, EventArgs e)
        {
            try
            {
                if (Regex.IsMatch(Input_BarrelTemp1.Text, regex))
                {
                    string temp = Input_BarrelTemp1.Text.Replace(".", "");
                    BarrelTemp1 = (short)(short.Parse(temp));
                }
                else
                {
                    BarrelTemp1 = (short)(short.Parse(Input_BarrelTemp1.Text) * 10);
                }

                if (Regex.IsMatch(Input_BarrelTemp2.Text, regex))
                {
                    string temp = Input_BarrelTemp2.Text.Replace(".", "");
                    BarrelTemp2 = (short)(short.Parse(temp));
                }
                else
                {
                    BarrelTemp2 = (short)(short.Parse(Input_BarrelTemp2.Text) * 10);
                }

                if (Regex.IsMatch(Input_BarrelTemp3.Text, regex))
                {
                    string temp = Input_BarrelTemp3.Text.Replace(".", "");
                    BarrelTemp3 = (short)(short.Parse(temp));
                }
                else
                {
                    BarrelTemp3 = (short)(short.Parse(Input_BarrelTemp3.Text) * 10);
                }

                if (Regex.IsMatch(Input_BarrelTemp4.Text, regex))
                {
                    string temp = Input_BarrelTemp4.Text.Replace(".", "");
                    BarrelTemp4 = (short)(short.Parse(temp));
                }
                else
                {
                    BarrelTemp4 = (short)(short.Parse(Input_BarrelTemp4.Text) * 10);
                }

                if (Regex.IsMatch(Input_BarrelTemp5.Text, regex))
                {
                    string temp = Input_BarrelTemp5.Text.Replace(".", "");
                    BarrelTemp5 = (short)(short.Parse(temp));
                }
                else
                {
                    BarrelTemp5 = (short)(short.Parse(Input_BarrelTemp5.Text) * 10);
                }
                DialogResult = DialogResult.OK;
            }
            catch
            {
                MessageBox.Show("解析数据失败，请检查数据重新输入");
            }

        }
    }
}
