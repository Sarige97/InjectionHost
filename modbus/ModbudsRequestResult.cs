using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.modbus
{
    internal class ModbudsRequestResult
    {
        public byte[] ResponseBytes { get; set; }
        public SlaveRequestResult SlaveRequestResult { get; set; }

        public String ExceptionMessage { get; set; }

        public ModbudsRequestResult(byte[] responseBytes, SlaveRequestResult slaveRequestResult)
        {
            this.ResponseBytes = responseBytes;
            this.SlaveRequestResult = slaveRequestResult;
        }
        public ModbudsRequestResult(byte[] responseBytes, SlaveRequestResult slaveRequestResult, string exceptionMessage)
        {
            this.ResponseBytes = responseBytes;
            this.SlaveRequestResult = slaveRequestResult;
            this.ExceptionMessage = exceptionMessage;
        }
    }
}
