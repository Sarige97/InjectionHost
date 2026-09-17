using moju.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace moju.log
{
    internal class SimpleLogger
    {
        /// <summary>
        /// 文件日志的绝对路径
        /// </summary>
        private static string _fileName;


        /// <summary>
        /// 文件日志所在文件夹的绝对路径
        /// </summary>
        private static string _directoryPath;

        public readonly static SimpleLogger Instance = new SimpleLogger();

        private readonly static string[] _logLevelArray = new string[5] { LogConstants.LogLevelDebug, LogConstants.LogLevelInfo, LogConstants.LogLevelWarn, LogConstants.LogLevelError, LogConstants.LogLevelFatal };

        private readonly static Object _fileLogLock = new Object();

        private readonly static Object _uiLogLock = new Object();

        public event EventHandler<string> LogEvent;

        private SimpleLogger()
        {
            // 每次程序启动，初始化一个日志文件，用初始化时的时间戳作为文件名
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _directoryPath = Path.Combine(baseDirectory, "log");
            _fileName = $"Log_{DateTime.Now:yyyy-MM-dd--HH-mm-ss}.log";
            // 确保文件夹存在
            Directory.CreateDirectory(_directoryPath);
        }

        public void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!shouldLog(LogConstants.LogLevelDebug))
            {
                return;
            }
            Log(LogConstants.LogLevelDebug, message, memberName, filePath, lineNumber);
        }

        public void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!shouldLog(LogConstants.LogLevelInfo))
            {
                return;
            }
            Log(LogConstants.LogLevelInfo, message, memberName, filePath, lineNumber);
        }

        public void Warn(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!shouldLog(LogConstants.LogLevelWarn))
            {
                return;
            }
            Log(LogConstants.LogLevelWarn, message, memberName, filePath, lineNumber);
        }

        public void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!shouldLog(LogConstants.LogLevelError))
            {
                return;
            }
            Log(LogConstants.LogLevelError, message, memberName, filePath, lineNumber);
        }


        public void Fatal(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!shouldLog(LogConstants.LogLevelFatal))
            {
                return;
            }
            Log(LogConstants.LogLevelFatal, message, memberName, filePath, lineNumber);
        }


        private void Log(string logLevel, string message, string memberName, string filePath, int lineNumber)
        {
            string className = Path.GetFileNameWithoutExtension(filePath);

            int[] outputTargetList = ConfigManager.Instance.OutputTargetList;
            bool needFileLog = Array.IndexOf(outputTargetList, LogConstants.LogOutputTargetFileLog) != -1;
            bool needConsleLog = Array.IndexOf(outputTargetList, LogConstants.LogOutputTargetConsole) != -1;
            bool needUiLog = Array.IndexOf(outputTargetList, LogConstants.LogOutputTargetUiConsole) != -1;
            bool needDatabaseLog = Array.IndexOf(outputTargetList, LogConstants.LogOutputTargetDataBase) != -1;

            string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}][{logLevel}][{className}.{memberName}:{lineNumber}]{message}";

            try
            {
                if (needFileLog)
                {
                    lock (_fileLogLock)
                    {
                        File.AppendAllText(Path.Combine(_directoryPath, _fileName), logContent + Environment.NewLine, Encoding.UTF8);
                    }
                }

                if (needUiLog)
                {
                    lock (_uiLogLock)
                    {
                        LogEvent.Invoke(this, logContent);
                    }
                }

                if (needConsleLog)
                {
                    // 用没有Environment.NewLine新起一行的模板
                    string consoleLogContent = logContent;

                    Console.WriteLine(consoleLogContent);
                }

                if (needDatabaseLog)
                {
                    //
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("记录日志时出错:" + e.Message);
            }


        }

        private bool shouldLog(string logLevel)
        {
            int index = Array.IndexOf(_logLevelArray, logLevel);
            string systemLogLevel = ConfigManager.Instance.LogLevel;
            int systemLogLevelIndex = Array.IndexOf(_logLevelArray, systemLogLevel);
            return index >= systemLogLevelIndex;
        }

    }
}
