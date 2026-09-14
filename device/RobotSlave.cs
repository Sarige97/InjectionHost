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
    internal class RobotSlave : ModbusSlaveTcp
    {

        /// <summary>
        /// 电源
        /// </summary>
        public bool Power
        {
            get
            {
                return GetAttributeByRegionAndAddress(0, 0).GetBool();
            }
        }

        /// <summary>
        /// 就绪（电源）
        /// </summary>
        public bool Ready
        {
            get
            {
                return GetAttributeByRegionAndAddress(1, 0).GetBool();
            }
        }


        /// <summary>
        /// 夹持到位
        /// </summary>
        public bool PartGripped
        {
            get
            {
                return GetAttributeByRegionAndAddress(1, 1).GetBool();
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
        /// 运行状态字 bit0电源 bit1就绪 bit2取件 bit3放件 bit4回位 bit5故障 bit6自动
        /// </summary>
        public ushort RunStatusWord
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 0).GetUshort();
            }
        }

        /// <summary>
        /// 报警码
        /// </summary>
        public ushort AlarmCode
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 1).GetUshort();
            }
        }

        /// <summary>
        /// 循环时间
        /// </summary>
        public string CycleTime
        {
            get
            {
                ushort cycleTime = GetAttributeByRegionAndAddress(3, 2).GetUshort();
                return Convert.ToString(cycleTime / 10.0f) + "s";
            }
        }
        
        /// <summary>
        /// 累计取件数
        /// </summary>
        public string TotalPicks
        {
            get
            {
                return Convert.ToString(GetAttributeByRegionAndAddress(3, 3).GetUshort());
            }
        }

        /// <summary>
        /// 取出成功数
        /// </summary>
        public string PickSuccess
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 5).GetUshort() + "个";
            }
        }

        /// <summary>
        /// 取出失败数
        /// </summary>
        public string PickFail
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 7).GetUshort() + "个";
            }
        }

        /// <summary>
        /// 住区状态 0空/1夹持
        /// </summary>
        public ushort GripStatus
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 12).GetUshort();
            }
        }

        /// <summary>
        /// 瞬时功率(kw)
        /// </summary>
        public ushort InstantaneousPower
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 13).GetUshort();
            }
        }

        /// <summary>
        /// 当前动作号 0待机 1取件 2放件 3回位
        /// </summary>
        public ushort CurrentActionId
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 14).GetUshort();
            }
        }

        public RobotSlave(string ip, int port, int slaveId, List<SlaveAttribute> slaveAttributeList) : base(ip, port, slaveId, slaveAttributeList)
        {
        }

        public RobotSlave(string ip, int port, int slaveId, string catagoryName) : base(ip, port, slaveId, catagoryName)
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
