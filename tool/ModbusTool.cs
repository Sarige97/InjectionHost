using moju.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.tool
{
    internal class ModbusTool
    {
        /// <summary>
        /// 按 region、Address 升序排序后，把 region 相同且 Address 连续（逐个 +1）的
        /// ModbusAddressInfo 分成一组，每组一个 ModbusAddressInfo[]，
        /// 整体用 List<ModbusAddressInfo[]> 返回。
        /// </summary>
        public static List<List<SlaveAttribute>> GroupByConsecutiveAddress(List<SlaveAttribute> list)
        {
            var result = new List<List<SlaveAttribute>>();
            if (list == null || list.Count == 0)
                return result;

            // 1. 先按 region 排，region 相同的再按 Address 升序排
            List<SlaveAttribute> sorted = list.OrderBy(x => x.Region).ThenBy(x => x.Address).ToList();

            // 2. 逐个扫描：region 相同且 Address 恰好 +1 才并入当前组
            var currentGroup = new List<SlaveAttribute> { sorted[0] };
            for (int i = 1; i < sorted.Count; i++)
            {
                bool consecutive = sorted[i].Region == sorted[i - 1].Region
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
                    currentGroup = new List<SlaveAttribute> { sorted[i] };
                }
            }
            // 最后一组收尾
            result.Add(currentGroup);

            return result;
        }

    }
}
