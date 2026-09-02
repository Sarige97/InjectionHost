using moju.config;
using moju.log;
using moju.modbus;
using muju.tool;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace muju.modbus
{
    internal class ModbusTCPClientManager
    {

        private readonly static ConcurrentDictionary<String, SlaveChannel> _slaveChannelMapping = new ConcurrentDictionary<string, SlaveChannel>();


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
        /// 请求modbus从站
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="requestBytes"></param>
        /// <returns></returns>
        public async static Task<ModbusRequestResult> ModbusRequestByBytes(String ip, int port, byte[] requestBytes)
        {
            String targetAddress = CombineAddress(ip, port);
            if (!_slaveChannelMapping.TryGetValue(targetAddress, out SlaveChannel slaveChannel))
            {
                // 映射表中没有状态机，初始化一个状态机, 加锁防止状态机初始化两次
                lock (_slaveChannelMapping)
                {
                    _slaveChannelMapping[targetAddress] = new SlaveChannel(ip, port);
                }
                slaveChannel = _slaveChannelMapping[targetAddress];
            }

            try
            {
                if (!slaveChannel.CanRequest())
                {
                    SimpleLogger.Instance.Debug("未到请求间隔，跳过请求");
                    return new ModbusRequestResult(new byte[0], SlaveRequestResult.Passed, "未到请求间隔，跳过请求");
                }

                Task<byte[]> requestTask = requestAndParse(slaveChannel, requestBytes);
                // 发起请求，设置超时
                SimpleLogger.Instance.Debug("开始请求");
                Task completeTask = await Task.WhenAny(requestTask, Task.Delay(ConfigManager.Instance.ModbusRequestTimeoutMs));
                if (completeTask == requestTask)
                {
                    // 没有超时
                    byte[] responseByte = await requestTask;
                    slaveChannel.OnSuccess();
                    return new ModbusRequestResult(responseByte, SlaveRequestResult.Success);
                }
                else
                {
                    // 超时
                    SimpleLogger.Instance.Error("请求超时，目标地址：" + CombineAddress(ip, port) + "  请求报文：" + BitConverter.ToString(requestBytes));
                    slaveChannel.OnTimeout();
                    _ = requestTask.ContinueWith(t => t.Exception);
                    return new ModbusRequestResult(new byte[0], SlaveRequestResult.Timeout, "请求超时");
                }


            }
            catch (InvalidOperationException e)
            {
                SimpleLogger.Instance.Error("请求失败,和目标的连接断开，目标地址：" + CombineAddress(ip, port) + "  请求报文：" + BitConverter.ToString(requestBytes));
                slaveChannel.OnFailed();
                // TODO 异常日志
                return new ModbusRequestResult(new byte[0], SlaveRequestResult.OtherError);

            }
            catch (Exception e)
            {
                SimpleLogger.Instance.Error("请求失败，目标地址：" + CombineAddress(ip, port) + "  请求报文：" + BitConverter.ToString(requestBytes));
                slaveChannel.OnFailed();
                // TODO 异常日志
                return new ModbusRequestResult(new byte[0], SlaveRequestResult.OtherError);
            }
            finally
            {
            }
        }

        private async static Task<byte[]> requestAndParse(SlaveChannel slaveChannel, byte[] requestBytes)
        {
            try
            {
                TcpClient tcpClient = await slaveChannel.GetTcpClient();
                NetworkStream stream = tcpClient.GetStream();

                await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                // 获取报文头
                Byte[] bytesHead = new Byte[7];

                //await stream.ReadAsync(bytesHead, 0, bytesHead.Length);
                await StreamTool.StreamReadAsync(stream, bytesHead);
                int dataLength = (bytesHead[4] << 8 | bytesHead[5]);
                // 获取报文体

                Byte[] bytesBody = new Byte[dataLength - 1];
                await StreamTool.StreamReadAsync(stream, bytesBody);

                // 组合报文
                Byte[] resultByte = new Byte[bytesHead.Length + bytesBody.Length];
                Buffer.BlockCopy(bytesHead, 0, resultByte, 0, bytesHead.Length);
                Buffer.BlockCopy(bytesBody, 0, resultByte, bytesHead.Length, bytesBody.Length);
                return resultByte;

            }
            catch
            {
                throw;
            }
        }
    }
}
