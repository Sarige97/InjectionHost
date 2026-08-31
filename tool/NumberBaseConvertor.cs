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
    }
}
