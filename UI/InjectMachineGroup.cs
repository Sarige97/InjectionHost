using moju.device;
using moju.domain;
using System;
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
    public partial class InjectMachineGroup : UserControl
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

        public event EventHandler<InjectMachineGroup> InjectionMachineGroupDetailButtonClick;


        public InjectMachineGroup()
        {
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
            Label_InjectMachine_Running.Text = _injectionMoldingMachineSlave.Running ? "运行" : "停止";
            Label_InjectMachine_Running.ForeColor = _injectionMoldingMachineSlave.Running ? Color.Green : Color.Orange;
            LabelBox_InjectMachine_TotalShots.RightLabelText = _injectionMoldingMachineSlave.TotalShots;
            LabelBox_InjectMachine_AverageCycle.RightLabelText = _injectionMoldingMachineSlave.AveCycleTime;
            LabelBox_InjectMachine_MoldTemp.RightLabelText = _injectionMoldingMachineSlave.MoldTempActual;
            LabelBox_InjectMachine_HydraulicOilTemp.RightLabelText = _injectionMoldingMachineSlave.HydraulicOilTemp;
            LabelBox_InjectMachine_CoolingWaterTemp.RightLabelText = _injectionMoldingMachineSlave.CoolingWaterTemp;
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
            LabelBox_InjectMachine_Alert.ForeColor = alarmMsgColor;
            LabelBox_InjectMachine_Alert.RightLabelText = alarmDescription;
        }

        private void RefreshDryerData()
        {
            // 刷新干燥机数据
            Label_Dryer_Runing.Text = _dryerSlave.Running ? "运行" : "停止";
            LabelBox_Dryer_DewPoint.RightLabelText = _dryerSlave.DewPoint;
            LabelBox_Dryer_DryTemp.RightLabelText = _dryerSlave.DryTempActual;
            LabelBox_Dryer_MaterialLevel.RightLabelText = _dryerSlave.MaterialLevel;
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
            LabelBox_Dryer_Alert.RightLabelText = alarmMessage;
            LabelBox_Dryer_Alert.ForeColor = alarmColor;
        }

        private void RefreshRobotData()
        {
            Label_Robot_RunningStatus.Text = _robotSlave.Power ? "运行" : "停止";
            LabelBox_Robot_TotalPick.RightLabelText = _robotSlave.TotalPicks;
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
            LabelBox_Robot_Alert.RightLabelText = alarmMessage;
            LabelBox_Robot_Alert.ForeColor = alarmColor;

        }

        private void RefreshMoldTempMachineData()
        {
            Label_MoldTempMachine_RunningStatus.Text = _moldTemperatureControllerSlave.Running ? "运行" : "停止";
            LabelBox_MoldTempMachine_TempActual.RightLabelText = _moldTemperatureControllerSlave.TempActual;
            LabelBox_MoldTempMachine_ReturnWaterTemp.RightLabelText = _moldTemperatureControllerSlave.ReturnWaterTemp;
            LabelBox_MoldTempMachine_PumpPressure.RightLabelText = _moldTemperatureControllerSlave.PumpPressure;
            LabelBox_MoldTempMachine_PumpFlow.RightLabelText = _moldTemperatureControllerSlave.PumpFlow;
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
            LabelBox_MoldTempMachine_Alert.RightLabelText = alarmMessage;
            LabelBox_MoldTempMachine_Alert.ForeColor = alarmColor;

        }


        private void InjectMachineDetailButton_Click(object sender, EventArgs e)
        {
            // 发布点击事件让父层级处理
            InjectionMachineGroupDetailButtonClick.Invoke(sender, this);
        }
    }
}
