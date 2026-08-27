using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace muju.tool
{
    internal class StreamTool
    {

        public static Byte[] StreamRead(Stream stream, byte[] bytes, int timeOutTime)
        {
            long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            int readed = 0;
            while (true)
            {
                int read = stream.Read(bytes, readed, bytes.Length - readed);
                readed += read;

                if (readed >= bytes.Length)
                {
                    return bytes;
                }

                long nowTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if ((nowTime - startTime) > timeOutTime)
                {
                    throw new Exception("流读取超时");
                }

            }
        }

    }
}
