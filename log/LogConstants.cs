using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.log
{
    internal class LogConstants
    {
        public const string LogLevelDebug = "DEBUG";
        public const string LogLevelInfo = "INFO";
        public const string LogLevelWarn = "WARN";
        public const string LogLevelError = "ERROR";
        public const string LogLevelFatal = "FATAL";

        public const int LogOutputTargetFileLog = 0;
        public const int LogOutputTargetConsole = 1;
        public const int LogOutputTargetUiConsole = 2;
        public const int LogOutputTargetDataBase = 2;
    }
}
