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
    internal class DryerSlave : ModbusSlaveTcp
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
        /// 风机
        /// </summary>
        public bool FanOn
        {
            get
            {
                return GetAttributeByRegionAndAddress(0, 2).GetBool();
            }
        }

        /// <summary>
        /// 加热
        /// </summary>
        public bool HeatingOn
        {
            get
            {
                return GetAttributeByRegionAndAddress(0, 3).GetBool();
            }
        }

        /// <summary>
        /// 报警
        /// </summary>
        public bool Alarm
        {
            get
            {
                return GetAttributeByRegionAndAddress(1, 3).GetBool();
            }
        }

        /// <summary>
        /// 运行状态字 bit0电源 bit1运行 bit2风机 bit3加热 bit4料位低 bit5报警
        /// </summary>
        public ushort RunStatusWord
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 0).GetUshort();
            }
        }

        /// <summary>
        /// 报警码 0无/1露点高/2超温
        /// </summary>
        public ushort AlarmCode
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 1).GetUshort();
            }
        }

        /// <summary>
        /// 干燥温度实际
        /// </summary>
        public ushort DryTempActual
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 2).GetUshort();
            }
        }

        /// <summary>
        /// 露点
        /// </summary>
        public ushort DewPoint
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 3).GetUshort();
            }
        }

        /// <summary>
        /// 料点
        /// </summary>
        public ushort MeterialLevel
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 4).GetUshort();
            }
        }

        /// <summary>
        /// 加热功率(kW)
        /// </summary>
        public ushort HeatingPower
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 5).GetUshort();
            }
        }

       /// <summary>
       /// 风机电流
       /// </summary>
        public ushort FanCurrent
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 6).GetUshort();
            }
        }

        /// <summary>
        /// 干燥温度设定
        /// </summary>
        public ushort DryTempSetPoint
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 0).GetUshort();
            }
        }

        public DryerSlave(string ip, int port, int slaveId, List<SlaveAttribute> slaveAttributeList) : base(ip, port, slaveId, slaveAttributeList)
        {
        }

        public DryerSlave(string ip, int port, int slaveId, string catagoryName) : base(ip, port, slaveId, catagoryName)
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
