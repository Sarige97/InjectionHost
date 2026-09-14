using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.tool
{
    internal class NumberBaseConvertor
    {

        public static byte[] SplitUshort2Byte(ushort aByte)
        {
            byte s1 = (byte)(aByte >> 8);
            byte s2 = (byte)(aByte & 0xFF);
            return new byte[2] { s1, s2 };
        }

        public static ushort CombineTwoByte2Ushrot(byte b1, byte b2)
        {
            return (ushort)((b1 << 8) + b2);
        }

        public static byte[] UshortArray2ByteArray(ushort[] ushortArray)
        {
            byte[] resultBytes = new byte[ushortArray.Length * 2];
            for (int i = 0; i < ushortArray.Length; i++)
            {
                byte[] loopByte = SplitUshort2Byte(ushortArray[i]);
                resultBytes[2 * i] = loopByte[0];
                resultBytes[2 * i + 1] = loopByte[1];
            }
            return resultBytes;
        }

        public static byte[] BoolList2ByteArray(params bool[] bools)
        {
            // 计算bools要用几个字节表示
            int byteLength = (bools.Length / 8) + (bools.Length % 8 == 0 ? 0 : 1);
            BitArray bitArray = new BitArray(bools);

            byte[] resultBytes = new byte[byteLength];
            bitArray.CopyTo(resultBytes, 0);
            return resultBytes;

        }

        /// <summary>
        /// 将数值str转换为ushort保存,结果会将小数点乘10，例如输入“13.4”，会输出134
        /// </summary>
        /// <param name="str"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParseStr2Ushort10x(string str, out ushort result)
        {
            result = 0;
            if (!decimal.TryParse(str, out decimal decimalNum))
            {
                return false;
            }
            // 过滤范围
            if (decimalNum < -3276.8m || decimalNum > 3276.7m)
            {
                return false;
            }
            short temp = (short)Math.Round(decimalNum * 10m);
            result = (ushort)temp;
            return true;

        }

        /// <summary>
        /// 将数值str转换为ushort保存,结果会将小数点乘10
        /// </summary>
        /// <param name="str"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParseStr2Ushort(string str, out ushort result)
        {
            result = 0;
            if (!decimal.TryParse(str, out decimal decimalNum))
            {
                return false;
            }
            // 过滤范围
            if (decimalNum < -3276.8m || decimalNum > 3276.7m)
            {
                return false;
            }
            short temp = (short)Math.Round(decimalNum);
            result = (ushort)temp;
            return true;

        }

        /// <summary>
        /// 将两个ushort组合成一个int，例如将ABCD和1234组合成ABCD1234
        /// </summary>
        /// <param name="low">低字</param>
        /// <param name="high">高字</param>
        /// <returns></returns>
        public static int CombineTwoUshort2Int(ushort high, ushort low)
        {
            return ((int)high << 16) + ((int)low);
        }

        public static ushort[] SplitInt2UshortHighFirst(int integer)
        {
            ushort high = (ushort)(integer >> 16);
            ushort low = (ushort)(integer & 0xFFFF);
            return new ushort[2] { high, low };
        }

        public static ushort[] SplitInt2UshortLowFirst(int integer)
        {
            ushort high = (ushort)(integer >> 16);
            ushort low = (ushort)(integer & 0xFFFF);
            return new ushort[2] { low, high };
        }

        public static bool GetBitFromUshort(ushort u, int index)
        {
            if (index < 0 || index > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return (u & (1 << index)) != 0;
        }

    }
}
