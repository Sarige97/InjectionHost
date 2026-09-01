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

        /// <summary>
        /// 读取流到字节数组里，将字节数组读取满为止
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <param name="timeOutTime"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
