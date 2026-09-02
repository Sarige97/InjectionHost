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

        public SemaphoreSlim asyncLock { get; }

        public int nextAllowedTimeMs { get; private set; }

        public String ip { get; }

        public int port { get; }

        public SlaveChennelStatus status { get; private set; }

        public

        SlaveChannel(String ip, int port)
        {
            this.ip = ip;
            this.port = port;
            this.asyncLock = new SemaphoreSlim(1, 1);
            this.nextAllowedTimeMs = 0;
            this.failedCount = 0;
            this.tcpClient = new TcpClient();

            try
            {
                this.tcpClient.Connect(ip, port);
                this.status = SlaveChennelStatus.Ok;
            }
            catch (Exception e)
            {
                // TODO 日志，连接失败但是channel创建成功
                failedCount++;
                this.status = SlaveChennelStatus.ConnectFailed;

            }
        }

        public TcpClient GetTcpClient()
        {
            if (this.tcpClient != null)
            {
                return this.tcpClient;
            }
            else
            {
                return RecreateTcpClient();
            }
        }


        public TcpClient RecreateTcpClient()
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
                this.tcpClient.Connect(ip, port);
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
            nextAllowedTimeMs = GetNow() + ConfigManager.Instance.RetryArray[failedCount];
            status = SlaveChennelStatus.ReqeustFailed;
            CloseAndSetTcpClientNull();
        }

        public void OnSuccess()
        {
            failedCount = 0;
            nextAllowedTimeMs = 0;
            status = SlaveChennelStatus.Ok;
        }

        public void OnTimeout()
        {
            failedCount++;
            int RetryArrayIndex = failedCount > (ConfigManager.Instance.RetryArray.Length - 1) ? (ConfigManager.Instance.RetryArray.Length - 1) : failedCount;
            nextAllowedTimeMs = GetNow() + ConfigManager.Instance.RetryArray[RetryArrayIndex];
            status = SlaveChennelStatus.Timeout;
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
