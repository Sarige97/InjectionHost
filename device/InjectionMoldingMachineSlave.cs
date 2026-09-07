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
    internal class InjectionMoldingMachineSlave : ModbusSlave
    {
        // 运行 读写
        public bool Running
        {
            get
            {
                return GetAttributeByRegionAndAddress(0, 1).GetBool();
            }
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
        public ushort TotalShots
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 3).GetUshort();
            }
        }
        // 合格品数
        public ushort GoodParts
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 5).GetUshort();
            }
        }
        // 次品数
        public ushort RejectParts
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 7).GetUshort();
            }
        }
        // 料筒1段温度
        public ushort BarrelTemp1
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 19).GetUshort();
            }
        }
        // 料筒2段温度
        public ushort BarrelTemp2
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 20).GetUshort();
            }
        }
        // 料筒3段温度
        public ushort BarrelTemp3
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 21).GetUshort();
            }
        }
        // 料筒4段温度
        public ushort BarrelTemp4
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 22).GetUshort();
            }
        }
        // 料筒5段温度
        public ushort BarrelTemp5
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 23).GetUshort();
            }
        }

        // 模具温度
        public ushort MoldTempActual
        {
            get
            {
                return GetAttributeByRegionAndAddress(3, 29).GetUshort();
            }
        }
        // 料筒1段温度设定 读写
        public ushort BarrelSetpoint1
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 2).GetUshort();
            }
        }
        // 料筒2段温度设定 读写
        public ushort BarrelSetpoint2
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 3).GetUshort();
            }
        }
        // 料筒3段温度设定 读写
        public ushort BarrelSetpoint3
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 4).GetUshort();
            }
        }
        // 料筒4段温度设定 读写
        public ushort BarrelSetpoint4
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 5).GetUshort();
            }
        }
        // 模具设定温度 读写
        public ushort MoldTempSetpoint
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 6).GetUshort();
            }
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
                                FillResultInMapping<bool>(writableCoilMapping, SlaveAttributeList);
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
                                FillResultInMapping<bool>(readonlyCoilMapping, SlaveAttributeList);
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
                                FillResultInMapping<ushort>(writableRegisterMapping, SlaveAttributeList);
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
                                FillResultInMapping<ushort>(readOnlyRegisterMapping, SlaveAttributeList);
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
