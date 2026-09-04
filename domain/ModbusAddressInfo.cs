using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.domain
{
    internal class ModbusAddressInfo
    {
        public string Name { get; set; }
        public int region { get; set; }
        public ushort Address { get; set; }
        public string Description { get; set; }
        public int dotIndex { get; set; }
        public string unit { get; set; }
    }
}
