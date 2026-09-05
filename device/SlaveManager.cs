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
        public static SlaveManager Instance { get; private set; } = new SlaveManager();

        /// <summary>
        /// 读取json配置后，按机器将配置保存在这里
        /// </summary>
        private static Dictionary<string, List<SlaveAttribute>> _modbusAddressInfoListMapping = new Dictionary<string, List<SlaveAttribute>>();

        /// <summary>
        /// 根据ip:port-slaveId为键，保存从站实例
        /// </summary>
        private static Dictionary<string, ModbusSlave> _slaveMapping = new Dictionary<string, ModbusSlave>();


        static SlaveManager()
        {
            // 读取json配置保存在这个类
            string jsonAddress = ConfigManager.Instance.ModbusMappingJsonAddress;
            string absoluteAddress = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonAddress);
            string jsonContent = File.ReadAllText(absoluteAddress);
            JObject root = JObject.Parse(jsonContent);
            // 解析成型机的json
            JToken injectionMoldingMachine = root["InjectionMoldingMachine"];
            JToken mapping = injectionMoldingMachine["mapping"];
            List<SlaveAttribute> injectionMoldingMachineList = ((JObject)mapping).Properties().Select(x => x.Value.ToObject<SlaveAttribute>()).ToList();
            _modbusAddressInfoListMapping.Add("InjectionMoldingMachine", injectionMoldingMachineList);
        }

        public SlaveManager()
        {

        }

        public static List<SlaveAttribute> GetModbusMapping(string catagoryName)
        {
            return _modbusAddressInfoListMapping[catagoryName];
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
        public T GetOrCreateSlave<T>(string ip, int port, int slaveId, string catagory, Func<string, int, int, string, T> factory) where T : ModbusSlave
        {
            
            if (_slaveMapping.TryGetValue(GetModbusSlaveKey(ip, port, slaveId), out ModbusSlave modbusSlave))
            {
                // 获取到了直接返回
                return (T)modbusSlave;
            }
            else
            {
                List<SlaveAttribute> slaveAttributeList = _modbusAddressInfoListMapping[catagory];
                ModbusSlave tempModbusSlave = factory(ip, port, slaveId, catagory);
                _slaveMapping.Add(GetModbusSlaveKey(ip, port, slaveId), tempModbusSlave);
                return (T)tempModbusSlave;
            }
        }

        private string GetModbusSlaveKey(string ip, int port, int slaveId)
        {
            return $"{ip}:{port}-{slaveId}";
        }
    }
}
