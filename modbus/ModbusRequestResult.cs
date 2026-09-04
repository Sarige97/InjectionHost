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

        public SlaveChannelStatus SlaveChannelStatus { get; set; }

        public String ExceptionMessage { get; set; }

        public ModbusRequestResult(byte[] responseBytes, SlaveRequestResult slaveRequestResult, SlaveChannelStatus slaveChannelStatus)
        {
            this.ResponseBytes = responseBytes;
            this.SlaveRequestResult = slaveRequestResult;
            this.SlaveChannelStatus = slaveChannelStatus;
        }
        public ModbusRequestResult(byte[] responseBytes, SlaveRequestResult slaveRequestResult, SlaveChannelStatus slaveChannelStatus, string exceptionMessage)
        {
            this.ResponseBytes = responseBytes;
            this.SlaveRequestResult = slaveRequestResult;
            this.SlaveChannelStatus = slaveChannelStatus;
            this.ExceptionMessage = exceptionMessage;
        }
    }
}
