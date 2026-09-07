using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.domain
{
    internal class SlaveDeviceInfo
    {
        public string SlaveCatagoryName { set; get; }
        public string Ip { set; get; }
        public int Port { set; get; }
        public int SlaveId { set; get; }

        public List<SlaveAttribute> SlaveAttributeList = new List<SlaveAttribute>();

        public SlaveDeviceInfo()
        {
        }

        public SlaveDeviceInfo(string ip, int port, int slaveId)
        {
            Ip = ip;
            Port = port;
            SlaveId = slaveId;
        }
    }
}
