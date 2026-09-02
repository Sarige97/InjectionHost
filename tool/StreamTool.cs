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
        public static async Task<Byte[]> StreamReadAsync(Stream stream, byte[] bytes)
        {
            int readed = 0;
            while (true)
            {
                int read = await stream.ReadAsync(bytes, readed, bytes.Length - readed);
                readed += read;

                if (readed >= bytes.Length)
                {
                    return bytes;
                }
            }
        }

    }
}
