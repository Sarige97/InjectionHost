using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.modbus
{
    public enum SlaveChannelStatus
    {
        Ok,
        ConnectFailed,
        ReqeustFailed,
        Timeout,
        OtherError
    }

}
