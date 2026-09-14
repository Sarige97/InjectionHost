using moju.config;
using moju.device.interfaces;
using moju.domain;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.device
{
    internal class SlaveManager
    {
        /// <summary>
        /// 读取json配置后，按机器将配置保存在这里
        /// </summary>
        private static Dictionary<string, List<SlaveDeviceInfo>> _modbusAddressInfoListMapping = new Dictionary<string, List<SlaveDeviceInfo>>();

        /// <summary>
        /// 根据ip:port-slaveId为键，保存从站实例
        /// </summary>
        private static Dictionary<string, ModbusSlaveTcp> _slaveMapping = new Dictionary<string, ModbusSlaveTcp>();

        private static readonly Object _lock = new object();


        static SlaveManager()
        {
            // 读取json配置保存在这个类
            string jsonAddress = ConfigManager.Instance.ModbusMappingJsonAddress;
            string absoluteAddress = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonAddress);
            string jsonContent = File.ReadAllText(absoluteAddress);
            JObject root = JObject.Parse(jsonContent);
            foreach (JProperty property in root.Properties())
            {
                JObject jObject = property.Value as JObject;
                // 机器分类名称
                string name = property.Name;
                // 设备信息
                List<SlaveDeviceInfo> slaveDeviceInfoList = ((JObject)jObject["instance"]).Properties().Select(x => x.Value.ToObject<SlaveDeviceInfo>()).ToList();
                // 设备名
                List<string> nameList = ((JObject)jObject["instance"]).Properties().Select(x => x.Name).ToList();
                // 将设备名填充到信息中
                for (int i = 0; i < slaveDeviceInfoList.Count; i++)
                {

                    List<SlaveAttribute> slaveAttributeList = ((JObject)jObject["mapping"]).Properties().Select(x => x.Value.ToObject<SlaveAttribute>()).ToList();
                    slaveDeviceInfoList[i].SlaveCatagoryName = name;
                    slaveDeviceInfoList[i].SlaveAttributeList = slaveAttributeList;
                    slaveDeviceInfoList[i].name = nameList[i];
                }
                _modbusAddressInfoListMapping.Add(name, slaveDeviceInfoList);
            }

        }

        public SlaveManager()
        {

        }

        public static List<SlaveAttribute> GetModbusMapping(string ip, int port, int slaveId, string categoryName)
        {
            List<SlaveDeviceInfo> slaveDeviceInfos = _modbusAddressInfoListMapping[categoryName];
            foreach (SlaveDeviceInfo slaveDeviceInfo in slaveDeviceInfos)
            {
                if (ip != null && ip.Equals(slaveDeviceInfo.Ip) && slaveDeviceInfo.Port == port && slaveDeviceInfo.SlaveId == slaveId)
                {
                    return slaveDeviceInfo.SlaveAttributeList;
                }
            }
            return null;
        }

        public static List<SlaveDeviceInfo> GetModbusDeviceInfo(string categoryName)
        {
            return _modbusAddressInfoListMapping[categoryName];
        }

        /// <summary>
        /// 根据json中的分类名和instance的key来获取对应的设备信息
        /// </summary>
        /// <param name="categoryName"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static SlaveDeviceInfo GetSlaveByCatagoryAndDeviceId(string categoryName, string deviceId)
        {
            if (string.IsNullOrEmpty(categoryName) || string.IsNullOrEmpty(deviceId))
            {
                return null;
            }
            List<SlaveDeviceInfo> slaveDeviceInfos = GetModbusDeviceInfo(categoryName);
            return slaveDeviceInfos.Where(x => deviceId.Equals(x.name)).ToArray()[0];
        }

        /// <summary>
        /// 工厂方法
        /// 创建一个从站实例，保存后返回。 如果该ip,port,slaveId的实例已存在则直接返回
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ip">ip</param>
        /// <param name="port">端口</param>
        /// <param name="slaveId">从站id</param>
        /// <param name="catagory">json配置中的机器名</param>
        /// <param name="factory">工厂方法，用于获取从站实例，返回实例必须继承ModbusSlave</param>
        /// <returns></returns>
        public static T GetOrCreateSlave<T>(SlaveDeviceInfo deviceInfo, Func<string, int, int, string, T> factory) where T : ModbusSlaveTcp
        {

            lock (_lock)
            {
                if (_slaveMapping.TryGetValue(GetModbusSlaveKey(deviceInfo.Ip, deviceInfo.Port, deviceInfo.SlaveId), out ModbusSlaveTcp modbusSlave))
                {
                    // 获取到了直接返回
                    return (T)modbusSlave;
                }
                else
                {
                    if (factory == null)
                    {
                        throw new ArgumentException($"指定从站未实例化,且没有输入工厂函数;ip:{deviceInfo.Ip},port:{deviceInfo.Port},slaveId:{deviceInfo.SlaveId},catagory:{deviceInfo.SlaveCatagoryName}");
                    }
                    ModbusSlaveTcp tempModbusSlave = factory(deviceInfo.Ip, deviceInfo.Port, deviceInfo.SlaveId, deviceInfo.SlaveCatagoryName);
                    _slaveMapping.Add(GetModbusSlaveKey(deviceInfo.Ip, deviceInfo.Port, deviceInfo.SlaveId), tempModbusSlave);
                    return (T)tempModbusSlave;
                }
            }
        }

        private static string GetModbusSlaveKey(string ip, int port, int slaveId)
        {
            return $"{ip}:{port}-{slaveId}";
        }
    }
}
