using System;
using System.Collections.Generic;
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

    }
}
