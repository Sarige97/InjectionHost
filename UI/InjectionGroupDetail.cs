using Microsoft.VisualBasic;
using moju.device;
using moju.domain;
using moju.tool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace moju.UI
{
    public partial class InjectionGroupDetail : UserControl
    {

        public string InjectMachineId { get; set; }

        public string MoldTempMachineId { get; set; }

        public string DryerId { get; set; }

        public string RobotId { get; set; }

        private InjectionMoldingMachineSlave _injectionMoldingMachineSlave;
        private MoldTemperatureControllerSlave _moldTemperatureControllerSlave;
        private DryerSlave _dryerSlave;
        private RobotSlave _robotSlave;

        private bool _inited = false;

        public event EventHandler<EventArgs> InjectionMachineGroupDetailBackButtonClick;


        public InjectionGroupDetail(string injectMachineId, string moldTempMachineId, string dryerId, string robotId)
        {
            this.InjectMachineId = injectMachineId;
            this.MoldTempMachineId = moldTempMachineId;
            this.DryerId = dryerId;
            this.RobotId = robotId;

            InitializeComponent();
        }

        public void Init()
        {
            if (_inited)
            {
                // 已经初始化则不要再次初始化
                return;
            }
            lock (this)
            {
                // 开发模式下跳过类型初始化
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    return;
                }
                // 初始化设备信息
                SlaveDeviceInfo _injectionMoldingMachineDeviceInfo = SlaveManager.GetSlaveByCatagoryAndDeviceId("InjectionMoldingMachine", InjectMachineId);
                SlaveDeviceInfo _moldTemperatureControllerDeviceInfo = SlaveManager.GetSlaveByCatagoryAndDeviceId("MoldTemperatureController", MoldTempMachineId);
                SlaveDeviceInfo _dryerDeviceInfo = SlaveManager.GetSlaveByCatagoryAndDeviceId("Dryer", DryerId);
                SlaveDeviceInfo _robotDeviceInfo = SlaveManager.GetSlaveByCatagoryAndDeviceId("Robot", RobotId);

                _injectionMoldingMachineSlave = SlaveManager.GetOrCreateSlave<InjectionMoldingMachineSlave>(_injectionMoldingMachineDeviceInfo, null);
                _moldTemperatureControllerSlave = SlaveManager.GetOrCreateSlave<MoldTemperatureControllerSlave>(_moldTemperatureControllerDeviceInfo, null);
                _dryerSlave = SlaveManager.GetOrCreateSlave<DryerSlave>(_dryerDeviceInfo, null);
                _robotSlave = SlaveManager.GetOrCreateSlave<RobotSlave>(_robotDeviceInfo, null);
                _inited = true;
            }

        }

        public void RefreshData()
        {
            if (!_inited)
            {
                Init();
            }
            // 刷新注塑机数据
            RefreshInjectMachineData();
            RefreshDryerData();
            RefreshRobotData();
            RefreshMoldTempMachineData();

        }



        private void RefreshInjectMachineData()
        {
            IM_Running.RightLabelText = _injectionMoldingMachineSlave.Running ? "运行" : "停止";
            IM_Running.ForeColor = _injectionMoldingMachineSlave.Running ? Color.Green : Color.Orange;

            ushort runStatusWord = _injectionMoldingMachineSlave.RunStatusWord;

            StringBuilder runStatusStrSB = new StringBuilder();
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 0) ? "电源/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 1) ? "合模/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 2) ? "注射/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 3) ? "保压/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 4) ? "熔胶/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 5) ? "冷却/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 6) ? "开模/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 7) ? "顶出/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 8) ? "故障/" : "");
            string runStatusStr = runStatusStrSB.ToString();
            if (runStatusStr.EndsWith("/"))
            {
                runStatusStr = runStatusStr.Substring(0, runStatusStr.Length - 1);
            }
            IM_RunningStatus.RightLabelText = runStatusStr;


            IM_TotalShot.RightLabelText = _injectionMoldingMachineSlave.TotalShots;
            IM_GoodParts.RightLabelText = _injectionMoldingMachineSlave.GoodParts;
            IM_RejectParts.RightLabelText = _injectionMoldingMachineSlave.RejectParts;

            string barrelTemp1 = _injectionMoldingMachineSlave.BarrelTemp1;
            string barrelTemp2 = _injectionMoldingMachineSlave.BarrelTemp2;
            string barrelTemp3 = _injectionMoldingMachineSlave.BarrelTemp3;
            string barrelTemp4 = _injectionMoldingMachineSlave.BarrelTemp4;
            string barrelTemp5 = _injectionMoldingMachineSlave.BarrelTemp5;

            IM_BarrelTemp.RightLabelText = new StringBuilder().Append(barrelTemp1).Append("/").Append(barrelTemp2).Append("/").Append(barrelTemp3).Append("/").Append(barrelTemp4).Append("/").Append(barrelTemp5).Append(" ℃").ToString();

            IM_AvgCycleTime.RightLabelText = _injectionMoldingMachineSlave.AveCycleTime;
            IM_MoldTempActual.RightLabelText = _injectionMoldingMachineSlave.MoldTempActual;
            IM_HydraulicOilTemp.RightLabelText = _injectionMoldingMachineSlave.HydraulicOilTemp;
            IM_CoolingWaterTemp.RightLabelText = _injectionMoldingMachineSlave.CoolingWaterTemp;
            IM_InjectionPressure.RightLabelText = _injectionMoldingMachineSlave.InjectionPressure;
            IM_InjectionSpeed.RightLabelText = _injectionMoldingMachineSlave.InjectionSpeed;
            ushort alarmCode = _injectionMoldingMachineSlave.AlarmCode;
            string alarmDescription;
            Color alarmMsgColor;
            switch (alarmCode)
            {
                case (0):
                    {
                        alarmDescription = "正常";
                        alarmMsgColor = Color.Green;
                        break;
                    }
                case (1):
                    {
                        alarmDescription = "油温过高";
                        alarmMsgColor = Color.Red;
                        break;
                    }
                case (2):
                    {
                        alarmDescription = "模具报警";
                        alarmMsgColor = Color.Red;
                        break;
                    }
                case (4):
                    {
                        alarmDescription = "射胶异常";
                        alarmMsgColor = Color.Red;
                        break;
                    }
                default:
                    {
                        alarmDescription = $"其他异常({alarmCode})";
                        alarmMsgColor = Color.Green;
                        break;
                    }
            }
            IM_Alert.ForeColor = alarmMsgColor;
            IM_Alert.RightLabelText = alarmDescription;
        }

        private void RefreshDryerData()
        {
            // 刷新干燥机数据
            DRY_Running.RightLabelText = _dryerSlave.Running ? "运行" : "停止";
            DRY_Running.ForeColor = _dryerSlave.Running ? Color.Green : Color.Orange;

            ushort runStatusWord = _dryerSlave.RunStatusWord;

            StringBuilder runStatusStrSB = new StringBuilder();
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 2) ? "风机/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 3) ? "加热/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 4) ? "料位低/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 5) ? "报警/" : "");
            string runStatusStr = runStatusStrSB.ToString();
            if (runStatusStr.EndsWith("/"))
            {
                runStatusStr = runStatusStr.Substring(0, runStatusStr.Length - 1);
            }

            DRY_RunningStatus.RightLabelText = runStatusStr;
            DRY_Drying.RightLabelText = _dryerSlave.Drying;
            DRY_DewPoint.RightLabelText = _dryerSlave.DewPoint;
            Dry_DryTempActual.RightLabelText = _dryerSlave.DryTempActual;
            DRY_MaterialLevel.RightLabelText = _dryerSlave.MaterialLevel;
            DRY_HeatingPower.RightLabelText = _dryerSlave.HeatingPower;
            DRY_FanCurrent.RightLabelText = _dryerSlave.FanCurrent;
            ushort alarmCode = _dryerSlave.AlarmCode;
            string alarmMessage = "";
            Color alarmColor = Color.Green;
            switch (alarmCode)
            {
                case (0):
                    {
                        alarmMessage = "正常";
                        alarmColor = Color.Green;
                        break;
                    }
                case (1):
                    {
                        alarmMessage = "露点高";
                        alarmColor = Color.Red;
                        break;
                    }
                case (2):
                    {
                        alarmMessage = "超温";
                        alarmColor = Color.Red;
                        break;
                    }
                default:
                    {
                        alarmMessage = "其他异常";
                        alarmColor = Color.Red;
                        break;
                    }
            }
            DRY_Alert.RightLabelText = alarmMessage;
            DRY_Alert.ForeColor = alarmColor;
        }

        private void RefreshRobotData()
        {
            ROB_Running.RightLabelText = _robotSlave.Power ? "运行" : "停止";
            ROB_Running.ForeColor = _robotSlave.Power ? Color.Green : Color.Orange;
            ROB_CycleTime.RightLabelText = _robotSlave.CycleTime;
            ROB_TotalPicks.RightLabelText = _robotSlave.TotalPicks;
            ROB_PickSuccess.RightLabelText = _robotSlave.PickSuccess;
            ROB_PickFail.RightLabelText = _robotSlave.PickFail;

            ushort runStatusWord = _robotSlave.RunStatusWord;

            StringBuilder runStatusStrSB = new StringBuilder();
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 0) ? "电源/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 1) ? "就绪/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 2) ? "取件/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 3) ? "放件/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 4) ? "回位/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 5) ? "故障/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 6) ? "自动/" : "");
            string runStatusStr = runStatusStrSB.ToString();
            if (runStatusStr.EndsWith("/"))
            {
                runStatusStr = runStatusStr.Substring(0, runStatusStr.Length - 1);
            }

            ROB_RunningStatus.RightLabelText = runStatusStr;


            ushort alarmCode = _robotSlave.AlarmCode;
            string alarmMessage = "";
            Color alarmColor = Color.Green;
            switch (alarmCode)
            {
                case (0):
                    {
                        alarmMessage = "正常";
                        alarmColor = Color.Green;
                        break;
                    }
                case (1):
                    {
                        alarmMessage = "取件失败";
                        alarmColor = Color.Red;
                        break;
                    }
                case (2):
                    {
                        alarmMessage = "机械故障";
                        alarmColor = Color.Red;
                        break;
                    }
                default:
                    {
                        alarmMessage = "其他异常";
                        alarmColor = Color.Red;
                        break;
                    }
            }
            ROB_Alert.RightLabelText = alarmMessage;
            ROB_Alert.ForeColor = alarmColor;

        }

        private void RefreshMoldTempMachineData()
        {
            MTC_Running.RightLabelText = _moldTemperatureControllerSlave.Running ? "运行" : "停止";
            MTC_Running.ForeColor = _moldTemperatureControllerSlave.Running ? Color.Green : Color.Orange;

            ushort runStatusWord = _moldTemperatureControllerSlave.RunStatusWord;

            StringBuilder runStatusStrSB = new StringBuilder();
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 0) ? "电源/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 1) ? "运行/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 2) ? "加热/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 3) ? "冷却/" : "");
            runStatusStrSB.Append(NumberBaseConvertor.GetBitFromUshort(runStatusWord, 4) ? "报警/" : "");
            string runStatusStr = runStatusStrSB.ToString();
            if (runStatusStr.EndsWith("/"))
            {
                runStatusStr = runStatusStr.Substring(0, runStatusStr.Length - 1);
            }

            MTC_RunningStatus.RightLabelText = runStatusStr;

            MTC_Heating.RightLabelText = _moldTemperatureControllerSlave.Heating;
            MTC_PumpCurrent.RightLabelText = _moldTemperatureControllerSlave.PumpCurrent;

            MTC_TempActual.RightLabelText = _moldTemperatureControllerSlave.TempActual;
            MTC_ReturnWaterTemp.RightLabelText = _moldTemperatureControllerSlave.ReturnWaterTemp;
            MTC_PumpPressure.RightLabelText = _moldTemperatureControllerSlave.PumpPressure;
            MTC_PumpFlow.RightLabelText = _moldTemperatureControllerSlave.PumpFlow;
            ushort alarmCode = _moldTemperatureControllerSlave.AlarmCode;
            string alarmMessage = "";
            Color alarmColor = Color.Green;
            switch (alarmCode)
            {
                case (0):
                    {
                        alarmMessage = "正常";
                        alarmColor = Color.Green;
                        break;
                    }
                case (1):
                    {
                        alarmMessage = "超温";
                        alarmColor = Color.Red;
                        break;
                    }
                case (2):
                    {
                        alarmMessage = "泵故障";
                        alarmColor = Color.Red;
                        break;
                    }
                default:
                    {
                        alarmMessage = "其他异常";
                        alarmColor = Color.Red;
                        break;
                    }
            }
            MTC_Alert.RightLabelText = alarmMessage;
            MTC_Alert.ForeColor = alarmColor;

        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            InjectionMachineGroupDetailBackButtonClick.Invoke(this, EventArgs.Empty);
        }



        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void IM_RunningStatus_Load(object sender, EventArgs e)
        {

        }

        private void standardDoubleLabel2_Load(object sender, EventArgs e)
        {

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            using (SetBarrelTemp form = new SetBarrelTemp(_injectionMoldingMachineSlave))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // 模态框输入完毕，拿返回值处理
                    if (await _injectionMoldingMachineSlave.SetBarrelTemp(form.BarrelTemp1, form.BarrelTemp2, form.BarrelTemp3, form.BarrelTemp4, form.BarrelTemp5))
                    {
                        MessageBox.Show("设定成功");
                    }
                    else
                    {
                        MessageBox.Show("设定失败，未知原因");
                    }
                }
            }
        }

        private void IM_Btn_SetMoldTempActual_Click(object sender, EventArgs e)
        {

        }

        private void standardDoubleLabel1_Load(object sender, EventArgs e)
        {

        }

        private void InjectionGroupDetail_Load(object sender, EventArgs e)
        {

        }

        private void ROB_RunningStatus_Load(object sender, EventArgs e)
        {

        }

        private void IM_NameLabel_Click(object sender, EventArgs e)
        {

        }

        private async void IM_Btn_SetMoldTempActual_Click_1(object sender, EventArgs e)
        {
            string temp = Interaction.InputBox("请输入目标温度，整数", "温度设置", "", -1, -1);
            if (short.TryParse(temp, out short result))
            {
                bool setResult = await _injectionMoldingMachineSlave.SetMoldTempSetpoint((ushort)(result * 10));
                if (setResult)
                {
                    MessageBox.Show("设置成功");
                }
                else
                {
                    MessageBox.Show("设定失败");
                }
            }
            else
            {
                MessageBox.Show("输入的值不合法");
            }
        }

        private async void DRY_Btn_SetDryTempActual_Click(object sender, EventArgs e)
        {
            string temp = Interaction.InputBox("请输入目标温度，整数", "温度设置", "", -1, -1);
            if (short.TryParse(temp, out short result))
            {
                bool setResult = await _dryerSlave.SetDryTempSetPoint((ushort)(result * 10));
                if (setResult)
                {
                    MessageBox.Show("设置成功");
                }
                else
                {
                    MessageBox.Show("设定失败");
                }
            }
            else
            {
                MessageBox.Show("输入的值不合法");
            }

        }

        private async void MTC_Btn_SetTemp_Click(object sender, EventArgs e)
        {
            string temp = Interaction.InputBox("请输入目标温度，整数", "温度设置", "", -1, -1);
            if (short.TryParse(temp, out short result))
            {
                bool setResult = await _moldTemperatureControllerSlave.SetTempSetPoint((ushort)(result * 10));
                if (setResult)
                {
                    MessageBox.Show("设置成功");
                }
                else
                {
                    MessageBox.Show("设定失败");
                }
            }
            else
            {
                MessageBox.Show("输入的值不合法");
            }

        }

        private void button_Click(object sender, EventArgs e)
        {
            HistoryTemp historyTemp = new HistoryTemp(_injectionMoldingMachineSlave);
            historyTemp.ShowDialog();
            historyTemp.Dispose();
        }
    }
}
