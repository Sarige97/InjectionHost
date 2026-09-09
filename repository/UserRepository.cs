using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using moju.domain;
using System.Data.SQLite;
using System.Security.Cryptography;

namespace moju.repository
{
    internal class UserRepository : BaseRepository
    {

        public static UserRepository Instance { get; } = new UserRepository();

        private UserRepository()
        {
        }



        /// <summary>
        /// 注册
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Insert(User user)
        {
            string sql = "insert into USER " +
               "(USERNAME, password, salt, realName, department, remark, status, extFiled1, extFiled2, extFiled3, extFiled4, extFiled5, extFiled6, extFiled7, extFiled8, extFiled9, extFiled10)" +
               " values " +
               "(@username, @password, @salt, @realname, @department, @remark, @status, @extFiled1, @extFiled2, @extFiled3, @extFiled4, @extFiled5, @extFiled6, @extFiled7, @extFiled8, @extFiled9, @extFiled10)";

            List<SQLiteParameter> parameters = new List<SQLiteParameter>();
            parameters.Add(new SQLiteParameter("username", user.username));
            parameters.Add(new SQLiteParameter("password", user.password));
            parameters.Add(new SQLiteParameter("salt", user.salt));
            parameters.Add(new SQLiteParameter("realname", user.realName));
            parameters.Add(new SQLiteParameter("department", user.department));
            parameters.Add(new SQLiteParameter("remark", user.remark));
            parameters.Add(new SQLiteParameter("status", user.status));
            parameters.Add(new SQLiteParameter("extFiled1", user.extFiled1));
            parameters.Add(new SQLiteParameter("extFiled2", user.extFiled2));
            parameters.Add(new SQLiteParameter("extFiled3", user.extFiled3));
            parameters.Add(new SQLiteParameter("extFiled4", user.extFiled4));
            parameters.Add(new SQLiteParameter("extFiled5", user.extFiled5));
            parameters.Add(new SQLiteParameter("extFiled6", user.extFiled6));
            parameters.Add(new SQLiteParameter("extFiled7", user.extFiled7));
            parameters.Add(new SQLiteParameter("extFiled8", user.extFiled8));
            parameters.Add(new SQLiteParameter("extFiled9", user.extFiled9));
            parameters.Add(new SQLiteParameter("extFiled10", user.extFiled10));

            return await DoExecute(sql, parameters.ToArray());

        }

        public async Task<User> SelectUserByUsername(string username)
        {
            string sql = "select username, password, salt, realName, department, remark, status, extFiled1, extFiled2, extFiled3, extFiled4, extFiled5, extFiled6, extFiled7, extFiled8, extFiled9, extFiled10 from USER where username = @username";
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }
            SQLiteParameter[] parameters = new SQLiteParameter[] { new SQLiteParameter("username", username) };
            QueryResult<User> queryResult = await DoQuery<User>(sql, parameters);
            if (queryResult.ResultList.Count > 0)
            {
                return queryResult.ResultList[0];
            }
            else
            {
                return null;
            }
        }

    }
}