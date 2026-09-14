using moju.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.domain
{
    public class SlaveAttribute
    {
        public string Name { get; set; }
        public int Region { get; set; }
        public ushort Address { get; set; }
        public string Description { get; set; }
        public int DotIndex { get; set; }
        public string Unit { get; set; }
        public object Value { get; set; }

        public ushort GetUshort()
        {
            try
            {
                if (Value == null)
                {
                    SimpleLogger.Instance.Error($"数值转化失败,名称:{Name} 区域:{Region} 地址:{Address} 描述:{Description}");
                    return new ushort();
                }
                return (ushort)Value;
            }
            catch (Exception e)
            {
                throw new FormatException($"数值转化为ushort失败。", e);
            }
        }

        public String GetString()
        {
            try
            {
                if (Value == null)
                {
                    SimpleLogger.Instance.Error($"数值转化失败,名称:{Name} 区域:{Region} 地址:{Address} 描述:{Description}");
                    return "";
                }
                return (String)Value;
            }
            catch (Exception e)
            {
                throw new FormatException($"数值转化为String失败", e);
            }
        }

        public bool GetBool()
        {
            try
            {
                if (Value == null)
                {
                    SimpleLogger.Instance.Error($"数值转化失败,名称:{Name} 区域:{Region} 地址:{Address} 描述:{Description}");
                    return false;
                }
                return (bool)Value;
            }
            catch (Exception e)
            {
                throw new FormatException($"数值转化为Bool失败", e);
            }

        }
    }
}
