using moju.log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace moju.config
{
    internal sealed class ConfigManager
    {
        public static ConfigManager Instance { get; private set; } = new ConfigManager();

        public int ModbusRequestTimeoutMs { get; private set; } = 2000;

        public string LogLevel { get; private set; } = "ERROR";

        public int[] OutputTargetList { get; private set; } = new int[3] { 0, 1, 2 };
        /// <summary>
        /// 定义超时次数与重试间隔的关系
        /// </summary>
        public int[] RetryArray { get; private set; } = new int[] { 1000, 2000, 2000, 4000, 4000, 8000, 15000, 30000, 60000, 300000 };

        public string errorMessage { get; private set; } = null;

        public int status { get; private set; } = -1;


        private ConfigManager()
        {
            // 初始化
            XmlDocument document = new XmlDocument();
            document.Load("config/config.xml");
            XmlNode xmlNode = document.SelectSingleNode("project");

            // 读取log配置
            XmlNode log = xmlNode.SelectSingleNode("Log");
            string logLevel = log.SelectSingleNode("LogLevel").InnerText;
            string[] logLevelArray = new string[5] { LogConstants.LogLevelDebug, LogConstants.LogLevelInfo, LogConstants.LogLevelWarn, LogConstants.LogLevelError, LogConstants.LogLevelFatal };
            if (Array.IndexOf(logLevelArray, logLevel) == -1)
            {
                status = 1;
                errorMessage = "初始化配置失败, 日志配置LogLevel初始化错误";
            }
            LogLevel = logLevel;

            string outputTarget = log.SelectSingleNode("outputTarget").InnerText;

            string[] outputTargetStringList = outputTarget.Split(',');
            int[] tempOutputTargetList = new int[outputTargetStringList.Length];
            for (int i = 0; i < outputTargetStringList.Length; i++)
            {
                if (int.TryParse(outputTargetStringList[i], out int looResult))
                {
                    tempOutputTargetList[i] = looResult;
                }
                else
                {
                    status = 1;
                    errorMessage = "初始化配置失败, 日志配置outputTarget初始化错误";
                    tempOutputTargetList = new int[0];
                    break;
                }
            }

            OutputTargetList = tempOutputTargetList;


            // 读取modbus协议配置
            XmlNode modbusProtocol = xmlNode.SelectSingleNode("modbusProtocol");

            string requestTimeoutMs = modbusProtocol.SelectSingleNode("requestTimeoutMs").InnerText;
            if (Int32.TryParse(requestTimeoutMs, out int result))
            {
                ModbusRequestTimeoutMs = result;
            }
            else
            {
                status = 1;
                errorMessage = "初始化配置失败, Modbus配置requestTimeoutMs初始化错误";
            }
            string retryArray = modbusProtocol.SelectSingleNode("retryArray").InnerText;
            string[] retryArrayString = retryArray.Split(',');
            int[] retryArrayInt = new int[retryArrayString.Length];
            for (int i = 0; i < retryArrayString.Length; i++)
            {
                if (int.TryParse(retryArrayString[i], out int loopResult))
                {
                    retryArrayInt[i] = loopResult;
                }
                else
                {
                    status = 1;
                    errorMessage = "初始化配置失败, Modbus配置retryArray初始化错误";
                    retryArrayInt = new int[0];
                    break;
                }
            }
            RetryArray = retryArrayInt;

            // 设置初始化成功标记
            status = 0;

        }





    }
}
