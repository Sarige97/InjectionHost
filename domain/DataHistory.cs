using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.domain
{
    public class DataHistory<T>
    {
        public DateTime dateTime;
        public T value;

        public DataHistory(DateTime dateTime, T value)
        {
            this.dateTime = dateTime;
            this.value = value;
        }
    }
}
