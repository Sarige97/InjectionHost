using moju.device.interfaces;
using moju.domain;
using moju.log;
using moju.tool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace moju.device
{
    public class InjectionMoldingMachineSlave : ModbusSlaveTcp
    {

        private SlaveAlarm _slaveAlarm = new SlaveAlarm();

        private SlaveAlarm _coilConnectAlarm = new SlaveAlarm();

        private SlaveAlarm _RegisterConnectAlarm = new SlaveAlarm();

        private SlaveAlarm _InputCoilConnectAlarm = new SlaveAlarm();

        private SlaveAlarm _InputRegisterConnectAlarm = new SlaveAlarm();

        private Dictionary<int, string> _alarmMessageMapping = new Dictionary<int, string>() {
            {0, "无"} ,
            {1, "油温过高"} ,
            {2, "模具报警"} ,
            {4, "射胶异常"} ,

        };
        public string SetAlarmCodeAndGetAlarmMessage(ushort alarmCode)
        {
            // 如果异常码没有发生变化，则不需要更新和消息
            if (alarmCode == _slaveAlarm.AlarmCode)
            {
                return null;
            }
            // 如果异常码从异常变为正常
            if (_slaveAlarm.AlarmCode != 0 && alarmCode == 0)
            {
                string alarmMessage = _alarmMessageMapping[_slaveAlarm.AlarmCode];
                _slaveAlarm.AlarmCode = alarmCode;
                return $"{alarmMessage}报警已解除";
            }
            // 如果从正常到异常
            if (_slaveAlarm.AlarmCode == 0 && alarmCode != 0)
            {
                string alarmMessage = _alarmMessageMapping[alarmCode];
                _slaveAlarm.AlarmCode = alarmCode;
                return $"{alarmMessage}报警";
            }
            return null;
        }


        public Queue<DataHistory<int>> HistoryTemp = new Queue<DataHistory<int>>();

        // 运行 读写
        public bool Running
        {
            get
            {
                return GetAttributeByRegionAndAddress(0, 1).GetBool();
            }
        }

        public async Task<bool> SetRunning(bool value)
        {
            return await WriteSingleCoil(1, value);
        }

        // 报警
        public bool Alarm
        {
            get
            {
                return GetAttributeByRegionAndAddress(1, 3).GetBool();
            }
        }
        // 运行状态字
        public ushort RunStatusWord
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 0).GetUshort();
            }
        }
        // 报警码
        public ushort AlarmCode
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 1).GetUshort();
            }
        }
        // 报警组字
        public ushort AlarmGroupWord
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 2).GetUshort();
            }
        }
        // 总模数
        public string TotalShots
        {
            get
            {
                ushort low = GetAttributeByRegionAndAddress(3, 3).GetUshort();
                ushort high = GetAttributeByRegionAndAddress(3, 4).GetUshort();
                int result = NumberBaseConvertor.CombineTwoUshort2Int(high, low);
                return result + "个";
            }
        }
        // 合格品数
        public string GoodParts
        {
            get
            {
                ushort low = GetAttributeByRegionAndAddress(3, 5).GetUshort();
                ushort high = GetAttributeByRegionAndAddress(3, 6).GetUshort();
                int result = NumberBaseConvertor.CombineTwoUshort2Int(high, low);
                return result + "个";
            }
        }
        // 次品数
        public string RejectParts
        {
            get
            {
                ushort low = GetAttributeByRegionAndAddress(3, 7).GetUshort();
                ushort high = GetAttributeByRegionAndAddress(3, 8).GetUshort();
                int result = NumberBaseConvertor.CombineTwoUshort2Int(high, low);
                return result + "个";
            }
        }

        // 平均生产周期
        public string AveCycleTime
        {
            get
            {
                ushort avgTime = GetAttributeByRegionAndAddress(3, 10).GetUshort();
                return Convert.ToString(avgTime / 10.0) + "s";
            }
        }
        // 料筒1段温度
        public string BarrelTemp1
        {
            get
            {
                return Convert.ToString(GetAttributeByRegionAndAddress(3, 19).GetUshort() / 10.0);
            }
        }
        // 料筒2段温度
        public string BarrelTemp2
        {
            get
            {
                return Convert.ToString(GetAttributeByRegionAndAddress(3, 20).GetUshort() / 10.0);
            }
        }
        // 料筒3段温度
        public string BarrelTemp3
        {
            get
            {
                return Convert.ToString(GetAttributeByRegionAndAddress(3, 21).GetUshort() / 10.0);
            }
        }
        // 料筒4段温度
        public string BarrelTemp4
        {
            get
            {
                return Convert.ToString(GetAttributeByRegionAndAddress(3, 22).GetUshort() / 10.0);
            }
        }
        // 料筒5段温度
        public string BarrelTemp5
        {
            get
            {
                return Convert.ToString(GetAttributeByRegionAndAddress(3, 23).GetUshort() / 10.0);
            }
        }

        // 模具温度
        public string MoldTempActual
        {
            get
            {
                return Convert.ToString(GetAttributeByRegionAndAddress(3, 29).GetUshort() / 10.0f) + "℃";
            }
        }

        public string HydraulicOilTemp
        {
            get
            {
                return Convert.ToString(GetAttributeByRegionAndAddress(3, 30).GetUshort() / 10.0f) + "℃";
            }
        }

        public string CoolingWaterTemp
        {
            get
            {
                return Convert.ToString(GetAttributeByRegionAndAddress(3, 31).GetUshort() / 10.0f) + "℃";
            }
        }
        // 料筒1段温度设定 读写
        public string BarrelSetpoint1
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 2).GetUshort().ToString();
            }
        }

        public async Task<bool> SetBarrelSetPoint1(ushort value)
        {
            return await WriteSingleRegister(2, value);
        }

        // 料筒2段温度设定 读写
        public string BarrelSetpoint2
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 3).GetUshort().ToString();
            }
        }

        public async Task<bool> SetBarrelSetPoint2(ushort value)
        {
            return await WriteSingleRegister(3, value);
        }

        // 料筒3段温度设定 读写
        public string BarrelSetpoint3
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 4).GetUshort().ToString();
            }
        }

        public async Task<bool> SetBarrelSetPoint3(ushort value)
        {
            return await WriteSingleRegister(4, value);
        }

        // 料筒4段温度设定 读写
        public string BarrelSetpoint4
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 5).GetUshort().ToString();
            }
        }

        public async Task<bool> SetBarrelSetPoint4(ushort value)
        {
            return await WriteSingleRegister(5, value);
        }

        // 料筒5段温度设定 读写
        public string BarrelSetpoint5
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 6).GetUshort().ToString();
            }
        }

        public async Task<bool> SetBarrelSetPoint5(ushort value)
        {
            return await WriteSingleRegister(6, value);
        }

        // 模具设定温度 读写
        public ushort MoldTempSetpoint
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 7).GetUshort();
            }
        }
        public async Task<bool> SetMoldTempSetpoint(ushort value)
        {
            return await WriteSingleRegister(7, value);
        }

        public string InjectionPressure
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 39).GetUshort() + "bar";
            }

        }

        public string InjectionSpeed
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 40).GetUshort() + "%";
            }

        }


        public async Task<bool> SetBarrelTemp(short temp1, short temp2, short temp3, short temp4, short temp5)
        {
            return await WriteMultiRegister(2, 5, new ushort[] { (ushort)(temp1), (ushort)(temp2), (ushort)(temp3), (ushort)(temp4), (ushort)(temp5) });
        }


        public InjectionMoldingMachineSlave(String ip, int port, int slaveId, List<SlaveAttribute> slaveAttributeList) : base(ip, port, slaveId, slaveAttributeList)
        {
        }

        public InjectionMoldingMachineSlave(String ip, int port, int slaveId, string catagoryName) : base(ip, port, slaveId, catagoryName)
        {
        }



        /// <summary>
        /// 自动请求modbus，读取所有数据
        /// </summary>
        public override async void RefreshData()
        {
            List<List<SlaveAttribute>> modbusRequestAddressGroup = ModbusTool.GroupByConsecutiveAddress(SlaveAttributeList);
            foreach (List<SlaveAttribute> modbusAddressesInfoList in modbusRequestAddressGroup)
            {
                // 判断当前list所在区域
                int region = modbusAddressesInfoList[0].Region;
                switch (region)
                {
                    case 0:
                        {
                            // 区域0，用可读写线圈逻辑

                            // 获取连续地址组的第一个地址
                            ushort firstAddress = modbusAddressesInfoList[0].Address;
                            Dictionary<ushort, bool> writableCoilMapping = await ReadWritableCoil(firstAddress, (ushort)modbusAddressesInfoList.Count);
                            if (writableCoilMapping != null)
                            {
                                FillResultInMapping<bool>(writableCoilMapping, region, SlaveAttributeList);
                            }

                            //// 只处理和之前状态不一样的
                            //// 之前读不到现在读到了
                            //if (_coilConnectAlarm.AlarmCode == -1 && writableCoilMapping != null)
                            //{
                            //    AlarmHandler($"modbus请求恢复，恢复从站注塑机{Ip}:{Port} slaveId:{SlaveId}");
                            //}
                            ////之前读到了，现在读不到
                            //if(_coilConnectAlarm.AlarmCode != -1 && writableCoilMapping == null)
                            //{
                            //    AlarmHandler($"modbus请求失败，失败从站注塑机{Ip}:{Port} slaveId:{SlaveId}");
                            //}

                            break;
                        }
                    case 1:
                        {
                            // 区域1，用离散输入线圈
                            // 获取连续地址组的第一个地址
                            ushort firstAddress = modbusAddressesInfoList[0].Address;
                            Dictionary<ushort, bool> readonlyCoilMapping = await ReadReadOnlyCoil(firstAddress, (ushort)modbusAddressesInfoList.Count);
                            if (readonlyCoilMapping != null)
                            {
                                FillResultInMapping<bool>(readonlyCoilMapping, region, SlaveAttributeList);
                            }

                            //// 只处理和之前状态不一样的
                            //// 之前读不到现在读到了
                            //if (_InputCoilConnectAlarm.AlarmCode == -1 && readonlyCoilMapping != null)
                            //{
                            //    AlarmHandler($"modbus请求恢复，恢复从站注塑机{Ip}:{Port} slaveId:{SlaveId}");
                            //}
                            ////之前读到了，现在读不到
                            //if (_InputCoilConnectAlarm.AlarmCode != -1 && readonlyCoilMapping == null)
                            //{
                            //    AlarmHandler($"modbus请求失败，失败从站注塑机{Ip}:{Port} slaveId:{SlaveId}");
                            //}

                            break;
                        }
                    case 4:
                        {
                            // 区域4，用保持寄存器逻辑
                            // 获取连续地址组的第一个地址
                            ushort firstAddress = modbusAddressesInfoList[0].Address;
                            Dictionary<ushort, ushort> writableRegisterMapping = await ReadWritableRegister(firstAddress, (ushort)modbusAddressesInfoList.Count);
                            if (writableRegisterMapping != null)
                            {
                                FillResultInMapping<ushort>(writableRegisterMapping, region, SlaveAttributeList);
                            }

                            //// 只处理和之前状态不一样的
                            //// 之前读不到现在读到了
                            //if (_RegisterConnectAlarm.AlarmCode == -1 && writableRegisterMapping != null)
                            //{
                            //    AlarmHandler($"modbus请求恢复，恢复从站注塑机{Ip}:{Port} slaveId:{SlaveId}");
                            //}
                            ////之前读到了，现在读不到
                            //if (_RegisterConnectAlarm.AlarmCode != -1 && writableRegisterMapping == null)
                            //{
                            //    AlarmHandler($"modbus请求失败，失败从站注塑机{Ip}:{Port} slaveId:{SlaveId}");
                            //}

                            break;
                        }
                    case 3:
                        {
                            // 区域3，用离散寄存器逻辑
                            // 获取连续地址组的第一个地址
                            ushort firstAddress = modbusAddressesInfoList[0].Address;
                            Dictionary<ushort, ushort> readOnlyRegisterMapping = await ReadReadOnlyRegister(firstAddress, (ushort)modbusAddressesInfoList.Count);
                            if (readOnlyRegisterMapping != null)
                            {
                                FillResultInMapping<ushort>(readOnlyRegisterMapping, region, SlaveAttributeList);
                            }

                            //// 只处理和之前状态不一样的
                            //// 之前读不到现在读到了
                            //if (_InputRegisterConnectAlarm.AlarmCode == -1 && readOnlyRegisterMapping != null)
                            //{
                            //    AlarmHandler($"modbus请求恢复，恢复从站注塑机{Ip}:{Port} slaveId:{SlaveId}");
                            //}
                            ////之前读到了，现在读不到
                            //if (_InputRegisterConnectAlarm.AlarmCode != -1 && readOnlyRegisterMapping == null)
                            //{
                            //    AlarmHandler($"modbus请求失败，失败从站注塑机{Ip}:{Port} slaveId:{SlaveId}");
                            //}


                            break;
                        }
                    default:
                        {
                            // todo  异常
                            SimpleLogger.Instance.Error("Modbus配置中又region不在0,1,4,3的区域");
                            break;
                        }
                }
            }
            try
            {
                // 获取模具实时温度，加入历史
                ushort temp = this.GetAttributeByRegionAndAddress(3, 29).GetUshort();
                // 判断队列中最老的数据是否距今一小时以上，如果是则移除，直到没有一小时以上的数据
                while (HistoryTemp.Count > 0)
                {
                    DateTime oldTime = HistoryTemp.Peek().dateTime;
                    TimeSpan timeSpan = DateTime.Now - oldTime;
                    if (timeSpan.TotalMinutes > 60)
                    {
                        HistoryTemp.Dequeue();
                    }
                    else
                    {
                        break;
                    }
                }
                // 将老数据移除完后加入新数据
                HistoryTemp.Enqueue(new DataHistory<int>(DateTime.Now, temp));
            }
            catch
            {
                SimpleLogger.Instance.Debug("将模具实时温度加入历史对象时报错");
            }

            try
            {
                string alarmMessage = SetAlarmCodeAndGetAlarmMessage(AlarmCode);
                if(!string.IsNullOrEmpty(alarmMessage))
                {
                    SimpleLogger.Instance.Error(alarmMessage);
                }
            } catch
            {
                SimpleLogger.Instance.Error("记录注塑机错误码时报错");
            }

        }
    }
}
