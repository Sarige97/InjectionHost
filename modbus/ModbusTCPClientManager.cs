using muju.tool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace muju.modbus
{
    internal class ModbusTCPClientManager
    {
        /// <summary>
        /// ip:port -> Tcpclient 映射关系
        /// </summary>
        private readonly static Dictionary<String, TcpClient> _tcpClientMapping = new Dictionary<string, TcpClient>();

        /// <summary>
        /// tcp客户端最近一次请求数据的时间
        /// </summary>
        private readonly static Dictionary<String, long> _recentRequestTime = new Dictionary<string, long>();

        private readonly static ConcurrentDictionary<String, Object> _createlockMapping = new ConcurrentDictionary<string, object>();
        private readonly static ConcurrentDictionary<String, Object> _requestlockMapping = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// 把ip和端口组合成ip:port的格式
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private static String CombineAddress(String ip, long port)
        {
            return ip + ":" + port.ToString();
        }

        /// <summary>
        /// 根据ip port获取对应连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private static TcpClient GetTcpClient(String ip, int port)
        {
            try
            {
                String targetAddress = CombineAddress(ip, port);
                object olock = _createlockMapping.GetOrAdd(targetAddress, new Object());
                lock (olock)
                {
                    if (_tcpClientMapping.TryGetValue(targetAddress, out TcpClient tcpClient))
                    {
                        if (tcpClient != null)
                        {
                            return tcpClient;
                        }
                        else
                        {
                            TcpClient newTcpClient = CreateConnectedTcpClient(ip, port);
                            _tcpClientMapping[targetAddress] = newTcpClient;
                            _recentRequestTime[targetAddress] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            return newTcpClient;
                        }
                    }
                    else
                    {
                        TcpClient newTcpClient = CreateConnectedTcpClient(ip, port);
                        _tcpClientMapping[targetAddress] = newTcpClient;
                        _recentRequestTime[targetAddress] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        return newTcpClient;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("获取tcp客户端失败, 目标地址：" + CombineAddress(ip, port));
                // TODO 异常日志
                return null;
            }
        }


        /// <summary>
        /// 请求modbus从站
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="requestBytes"></param>
        /// <returns></returns>
        public static Byte[] ModbusRequestByBytes(String ip, int port, Byte[] requestBytes)
        {
            try
            {
                String targetAddress = CombineAddress(ip, port);
                Object olock = _requestlockMapping.GetOrAdd(targetAddress, new Object());
                lock (olock)
                {
                    TcpClient tcpClient = GetTcpClient(ip, port);
                    NetworkStream stream = tcpClient.GetStream();
                    stream.Write(requestBytes, 0, requestBytes.Length);
                    // 获取报文头
                    Byte[] bytesHead = new Byte[7];
                    StreamTool.StreamRead(stream, bytesHead, 1000);
                    int dataLength = (bytesHead[4] << 8 | bytesHead[5]);
                    // 获取报文体
                    Byte[] bytesBody = new Byte[dataLength - 1];
                    StreamTool.StreamRead(stream, bytesBody, 1000);
                    // 组合报文
                    Byte[] resultByte = new Byte[bytesHead.Length + bytesBody.Length];
                    Buffer.BlockCopy(bytesHead, 0, resultByte, 0, bytesHead.Length);
                    Buffer.BlockCopy(bytesBody, 0, resultByte, bytesHead.Length, bytesBody.Length);

                    // 记录最后请求时间
                    _recentRequestTime[CombineAddress(ip, port)] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    return resultByte;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("请求失败，目标地址：" + CombineAddress(ip, port) + "  请求报文：" + BitConverter.ToString(requestBytes));
                // TODO 异常日志
                return null;

            }

        }

        // 创建一个已经连接的TcpClient
        private static TcpClient CreateConnectedTcpClient(String ip, int port)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(ip, port);
            return tcpClient;
        }




    }
}
