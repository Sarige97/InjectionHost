using moju.device.interfaces;
using moju.domain;
using moju.log;
using moju.tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.device
{
    internal class MoldTemperatureControllerSlave : ModbusSlaveTcp
    {
        /// <summary>
        /// 运行
        /// </summary>
        public bool Running
        {
            get
            {
                return GetAttributeByRegionAndAddress(0, 1).GetBool();
            }
        }

        /// <summary>
        /// 加热
        /// </summary>
        public bool HeaterOn
        {
            get
            {
                return GetAttributeByRegionAndAddress(0, 2).GetBool();
            }
        }

        /// <summary>
        /// 加热中
        /// </summary>
        public string Heating
        {
            get
            {
                return GetAttributeByRegionAndAddress(1, 1).GetBool()?"加热中":"未加热";
            }
        }

        /// <summary>
        /// 报警
        /// </summary>
        public bool Alarm
        {
            get
            {
                return GetAttributeByRegionAndAddress(1, 2).GetBool();
            }
        }

        /// <summary>
        /// 运行状态字 bit0电源 bit1运行 bit2加热 bit3冷却 bit4报警
        /// </summary>
        public ushort RunStatusWord
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 0).GetUshort();
            }
        }

        /// <summary>
        /// 报警码 0无/1超温/2泵故障
        /// </summary>
        public ushort AlarmCode
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 1).GetUshort();
            }
        }

        /// <summary>
        /// 实际温度
        /// </summary>
        public string TempActual
        {
            get
            {
                ushort temp = GetAttributeByRegionAndAddress(3, 2).GetUshort();
                return Convert.ToString(temp / 10.0f) + "℃";
            }
        }

        /// <summary>
        /// 回水温度
        /// </summary>
        public string ReturnWaterTemp
        {
            get
            {
                ushort temp = GetAttributeByRegionAndAddress(3, 3).GetUshort();
                return Convert.ToString(temp / 10.0f) + "℃";
            }
        }

        /// <summary>
        /// 泵压力
        /// </summary>
        public string PumpPressure
        {
            get
            {
                return Convert.ToString(GetAttributeByRegionAndAddress(3, 4).GetUshort()) + "bar";
            }
        }

        /// <summary>
        /// 泵流量
        /// </summary>
        public string PumpFlow
        {
            get
            {
                return Convert.ToString(GetAttributeByRegionAndAddress(3, 5).GetUshort()) + "L/min";
            }
        }

        /// <summary>
        /// 泵电流
        /// </summary>
        public string PumpCurrent
        {
            get
            {
                ushort current = GetAttributeByRegionAndAddress(3, 6).GetUshort();
                return Convert.ToString(current / 10.0f) + "A";
            }
        }

        /// <summary>
        /// 设定温度
        /// </summary>
        public ushort TempSetPoint
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 0).GetUshort();
            }
        }

        public async Task<bool> SetTempSetPoint(ushort value)
        {
            return await WriteSingleRegister(0, value);
        }

        public MoldTemperatureControllerSlave(string ip, int port, int slaveId, List<SlaveAttribute> slaveAttributeList) : base(ip, port, slaveId, slaveAttributeList)
        {
        }

        public MoldTemperatureControllerSlave(string ip, int port, int slaveId, string catagoryName) : base(ip, port, slaveId, catagoryName)
        {
        }

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
        }

    }
}
