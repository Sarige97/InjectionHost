using moju.config;
using moju.constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace moju.modbus
{
    internal class SlaveChannel
    {
        public TcpClient tcpClient { get; private set; }
        public int failedCount { get; private set; }

        public int nextAllowedTimeMs { get; private set; }

        public String ip { get; }

        public int port { get; }

        public SlaveChannelStatus status { get; private set; }

        public

        SlaveChannel(String ip, int port)
        {
            this.ip = ip;
            this.port = port;
            this.nextAllowedTimeMs = 0;
            this.failedCount = 0;
        }

        public async Task<TcpClient> GetTcpClient()
        {
            if (this.tcpClient != null)
            {
                return this.tcpClient;
            }
            else
            {
                return await RecreateTcpClient();
            }
        }


        public async Task<TcpClient> RecreateTcpClient()
        {
            // 重新创建tcpClient
            try
            {
                this.tcpClient.Close();
            }
            catch (Exception e)
            {
                // do nothing
            }
            finally
            {
                this.tcpClient = new TcpClient();
                Task connectTask = this.tcpClient.ConnectAsync(ip, port);
                Task completeTask = await Task.WhenAny(connectTask, Task.Delay(ConfigManager.Instance.ModbusRequestTimeoutMs));
                if (!(completeTask == connectTask))
                {
                    //超时
                    CloseAndSetTcpClientNull();
                    connectTask.ContinueWith(t => t.Exception);
                    this.tcpClient = null;
                    throw new InvalidOperationException();
                }
            }
            return tcpClient;

        }

        public int GetNow()
        {
            return Environment.TickCount;
        }

        public bool CanRequest()
        {
            return (GetNow() - nextAllowedTimeMs) > 0;
        }

        public void OnFailed()
        {
            failedCount++;
            int RetryArrayIndex = Math.Min(failedCount - 1, ConfigManager.Instance.RetryArray.Length - 1);
            nextAllowedTimeMs = GetNow() + ConfigManager.Instance.RetryArray[RetryArrayIndex];
            status = SlaveChannelStatus.ReqeustFailed;
            CloseAndSetTcpClientNull();
        }

        public void OnSuccess()
        {
            failedCount = 0;
            nextAllowedTimeMs = 0;
            status = SlaveChannelStatus.Ok;
        }

        public void OnTimeout()
        {
            failedCount++;
            int RetryArrayIndex = Math.Min(failedCount - 1, ConfigManager.Instance.RetryArray.Length - 1);
            nextAllowedTimeMs = GetNow() + ConfigManager.Instance.RetryArray[RetryArrayIndex];
            status = SlaveChannelStatus.Timeout;
            CloseAndSetTcpClientNull();
        }

        private void CloseAndSetTcpClientNull()
        {
            try
            {
                this.tcpClient.Close();
            }
            catch
            {

            }
            finally
            {
                this.tcpClient = null;
            }
        }
    }
}
