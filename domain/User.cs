using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moju.domain
{
    internal class User
    {
        public int id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string salt { get; set; }
        public string realName { get; set; }
        public string department { get; set; }
        public string remark { get; set; }
        public int status { get; set; }
        public string extFiled1 { get; set; }
        public string extFiled2 { get; set; }
        public string extFiled3 { get; set; }
        public string extFiled4 { get; set; }
        public string extFiled5 { get; set; }
        public string extFiled6 { get; set; }
        public string extFiled7 { get; set; }
        public string extFiled8 { get; set; }
        public string extFiled9 { get; set; }
        public string extFiled10 { get; set; }

        public User()
        {

        }

        public User(string username, string password, string salt, string realName, int status)
        {
            this.username = username ?? throw new ArgumentNullException(nameof(username));
            this.password = password ?? throw new ArgumentNullException(nameof(password));
            this.salt = salt ?? throw new ArgumentNullException(nameof(salt));
            this.realName = realName ?? throw new ArgumentNullException(nameof(realName));
            this.status = status;
        }

        public User(int id, string username, string password, string salt, string realName, string department, string remark, int status, string extFiled1, string extFiled2, string extFiled3, string extFiled4, string extFiled5, string extFiled6, string extFiled7, string extFiled8, string extFiled9, string extFiled10)
        {
            this.id = id;
            this.username = username ?? throw new ArgumentNullException(nameof(username));
            this.password = password ?? throw new ArgumentNullException(nameof(password));
            this.salt = salt ?? throw new ArgumentNullException(nameof(salt));
            this.realName = realName ?? throw new ArgumentNullException(nameof(realName));
            this.department = department;
            this.remark = remark;
            this.status = status;
            this.extFiled1 = extFiled1;
            this.extFiled2 = extFiled2;
            this.extFiled3 = extFiled3;
            this.extFiled4 = extFiled4;
            this.extFiled5 = extFiled5;
            this.extFiled6 = extFiled6;
            this.extFiled7 = extFiled7;
            this.extFiled8 = extFiled8;
            this.extFiled9 = extFiled9;
            this.extFiled10 = extFiled10;
        }
    }
}
