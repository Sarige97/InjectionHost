using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.device.interfaces
{
    internal abstract class ModbusServer
    {

        String ip { get; set; }
        int port { get; set; }

        public ModbusServer(String ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public byte[] readSingleCoil() 
        {

        }

        public byte[] readMultiCoil()
        {

        }
        public byte[] readSingleRegister()
        {

        }
        public byte[] readMultiRegister()
        {

        }
        public byte[] writeSingleCoil()
        {

        }
        public byte[] readSingleCoil()
        {

        }
        public byte[] readSingleCoil()
        {

        }
        public byte[] readSingleCoil()
        {

        }

    }
}
