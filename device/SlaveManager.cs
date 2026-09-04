using moju.config;
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

        private static Dictionary<string, List<ModbusAddressInfo>> _modbusAddressInfoListMapping = new Dictionary<string, List<ModbusAddressInfo>>();


        static SlaveManager()
        {
            string jsonAddress = ConfigManager.Instance.ModbusMappingJsonAddress;
            string absoluteAddress = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonAddress);
            string jsonContent = File.ReadAllText(absoluteAddress);
            JObject root = JObject.Parse(jsonContent);
            // 解析成型机的json
            JToken injectionMoldingMachine = root["InjectionMoldingMachine"];
            JToken mapping = injectionMoldingMachine["mapping"];
            List<ModbusAddressInfo> injectionMoldingMachineList = ((JObject)mapping).Properties().Select(x => x.Value.ToObject<ModbusAddressInfo>()).ToList();
            _modbusAddressInfoListMapping.Add("InjectionMoldingMachine", injectionMoldingMachineList);
        }

        public SlaveManager()
        {
        }

        public static List<ModbusAddressInfo> GetInjectionMoldingMachineAddressMapping()
        {
            return _modbusAddressInfoListMapping["InjectionMoldingMachine"];
        }

        /// <summary>
        /// 按 region、Address 升序排序后，把 region 相同且 Address 连续（逐个 +1）的
        /// ModbusAddressInfo 分成一组，每组一个 ModbusAddressInfo[]，
        /// 整体用 List<ModbusAddressInfo[]> 返回。
        /// </summary>
        public static List<List<ModbusAddressInfo>> GroupByConsecutiveAddress(List<ModbusAddressInfo> list)
        {
            var result = new List<List<ModbusAddressInfo>>();
            if (list == null || list.Count == 0)
                return result;

            // 1. 先按 region 排，region 相同的再按 Address 升序排
            List<ModbusAddressInfo> sorted = list.OrderBy(x => x.region).ThenBy(x => x.Address).ToList();

            // 2. 逐个扫描：region 相同且 Address 恰好 +1 才并入当前组
            var currentGroup = new List<ModbusAddressInfo> { sorted[0] };
            for (int i = 1; i < sorted.Count; i++)
            {
                bool consecutive = sorted[i].region == sorted[i - 1].region
                                && sorted[i].Address == sorted[i - 1].Address + 1;
                if (consecutive)
                {
                    currentGroup.Add(sorted[i]);
                }
                else
                {
                    // 断档/换区，当前组封口
                    result.Add(currentGroup);
                    // 开新组
                    currentGroup = new List<ModbusAddressInfo> { sorted[i] };     
                }
            }
            // 最后一组收尾
            result.Add(currentGroup);                                   

            return result;
        }

        /// <summary>
        /// 将地址信息转换为地址->地址信息的映射关系
        /// </summary>
        /// <param name="modbusAddressInfoList"></param>
        /// <returns></returns>
        public static Dictionary<ushort, ModbusAddressInfo> transferAddressList2Mapping(List<ModbusAddressInfo> modbusAddressInfoList)
        {
            Dictionary<ushort, ModbusAddressInfo> resultDictionary = new Dictionary<ushort, ModbusAddressInfo>();
            for (int i = 0; i < modbusAddressInfoList.Count; i++)
            {
                ModbusAddressInfo modbusAddressInfo = modbusAddressInfoList[i];
                resultDictionary.Add(modbusAddressInfo.Address, modbusAddressInfo);
            }
            return resultDictionary;
        }
    }
}
