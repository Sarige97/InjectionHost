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
    internal class WorkshopEnvironmentSlave : ModbusSlaveTcp
    {
        /// <summary>
        /// 高温报警
        /// </summary>
        public bool HighTempAlarm
        {
            get
            {
                return GetAttributeByRegionAndAddress(1, 0).GetBool();
            }
        }

        /// <summary>
        /// 高湿报警
        /// </summary>
        public bool HighHumidityAlarm
        {
            get
            {
                return GetAttributeByRegionAndAddress(1, 1).GetBool();
            }
        }

        /// <summary>
        /// 车间温度
        /// </summary>
        public string WorkshopTemp
        {
            get
            {
                ushort temp = GetAttributeByRegionAndAddress(3, 0).GetUshort();
                return Convert.ToString(((int)temp) / 10.0) + "℃";
            }
        }

        /// <summary>
        /// 湿度
        /// </summary>
        public string Humidity
        {
            get
            {
                ushort humidity = GetAttributeByRegionAndAddress(3, 1).GetUshort();
                return Convert.ToString((int)humidity / 10.0) + "%";
            }
        }

        /// <summary>
        /// 露点
        /// </summary>
        public string DewPoint
        {
            get
            {
                ushort dewPoint = GetAttributeByRegionAndAddress(3, 2).GetUshort();
                return Convert.ToString((int)dewPoint / 10.0) + "℃";

            }
        }

        /// <summary>
        /// 噪音
        /// </summary>
        public string NoiseLevel
        {
            get
            {
                ushort noiseLevel = GetAttributeByRegionAndAddress(3, 3).GetUshort();
                return Convert.ToString((int)noiseLevel / 10.0) + "dB";
            }
        }

        /// <summary>
        /// 空调设定温度
        /// </summary>
        public ushort HvacTempSetPoint
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 0).GetUshort();
            }
        }

        /// <summary>
        /// 目标湿度
        /// </summary>
        public ushort HumiditySetPoint
        {
            get
            {
                return GetAttributeByRegionAndAddress(4, 1).GetUshort();
            }
        }

        /// <summary>
        /// 设定空调温度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> setHvacTEmpSetPoint(ushort value)
        {
            return await WriteSingleRegister(0, value);
        }

        /// <summary>
        /// 设定目标湿度
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> setHumiditySetPoint(ushort value)
        {
            return await WriteSingleRegister(1, value);
        }



        public WorkshopEnvironmentSlave(string ip, int port, int slaveId, List<SlaveAttribute> slaveAttributeList) : base(ip, port, slaveId, slaveAttributeList)
        {
        }

        public WorkshopEnvironmentSlave(string ip, int port, int slaveId, string catagoryName) : base(ip, port, slaveId, catagoryName)
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
