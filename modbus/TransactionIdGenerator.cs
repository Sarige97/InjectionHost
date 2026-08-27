using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace muju.modbus
{
    internal class TransactionIdGenerator
    {

        private static readonly ConcurrentDictionary<String, ushort> _transactionIdMapping = new ConcurrentDictionary<string, ushort>();
        private static readonly ConcurrentDictionary<String, Object> _transactionIdLockMapping = new ConcurrentDictionary<string, Object>();

        /// <summary>
        /// 把ip和端口组合成ip:port的格式
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private static String CombineAddress(String ip, int port)
        {
            return ip + ":" + port.ToString();
        }

        public static ushort getTransactionIdByIp(String ip, int port)
        {
            unchecked
            {
                String target = CombineAddress(ip, port);
                object olock = _transactionIdLockMapping.GetOrAdd(target, new object());
                lock (olock)
                {
                    ushort id = _transactionIdMapping.GetOrAdd(target, 0x0000);
                    // 自增后塞回字典
                    _transactionIdMapping[target] = ++id;
                    // 返回原来的值
                    return id--;

                }
            }

        }

        /// <summary>
        /// 把ushort拆成两个byte
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static byte[] getTransactionIdBytesByIp(String ip, int port)// 1234
        {
            ushort transactionId = getTransactionIdByIp(ip, port);
            byte byte1 = (byte)(transactionId >> 8);
            byte byte2 = (byte)(transactionId & 0xFF);
            return new byte[2] { byte1, byte2 };
        }


    }
}
