using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.modbus
{
    internal class ModbusRequestResult
    {
        public byte[] ResponseBytes { get; set; }
        public SlaveRequestResult SlaveRequestResult { get; set; }

        public String ExceptionMessage { get; set; }

        public ModbusRequestResult(byte[] responseBytes, SlaveRequestResult slaveRequestResult)
        {
            this.ResponseBytes = responseBytes;
            this.SlaveRequestResult = slaveRequestResult;
        }
        public ModbusRequestResult(byte[] responseBytes, SlaveRequestResult slaveRequestResult, string exceptionMessage)
        {
            this.ResponseBytes = responseBytes;
            this.SlaveRequestResult = slaveRequestResult;
            this.ExceptionMessage = exceptionMessage;
        }
    }
}
