using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using moju.config;
using System.IO;
using System.Data.SQLite;
using moju.log;
using System.Data.Common;
using System.Collections.ObjectModel;
using moju.domain;

namespace moju.repository
{
    internal class BaseRepository
    {

        protected string ConnectionString { get; set; }

        protected BaseRepository()
        {
            string fileName = ConfigManager.Instance.FileName;
            string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            ConnectionString = $"Data Source={absolutePath}";
        }

        /// <summary>
        /// 动态参数的sql执行
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected async Task<bool> DoExecute(String sql)
        {
            SQLiteConnection connection = null;
            try
            {
                using (connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }

                }
                return true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance.Error("数据库执行报错:" + e.StackTrace);
                return false;
            }
            finally
            {
                try
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
                catch (Exception e)
                {
                    SimpleLogger.Instance.Error("数据库连接关闭失败:" + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// 动态参数的sql执行，示例（sql = "SELECT * FROM Users WHERE Age > @age", parameters = new SQLiteParameter("@age", 18)）
        /// </summary>
        /// <param name="sql">sql, 参数用@name占位</param>
        /// <param name="parameters">示例new SQLiteParameter("@age", 18)</param>
        /// <returns></returns>
        protected async Task<bool> DoExecute(String sql, SQLiteParameter[] parameters)
        {
            SQLiteConnection connection = null;
            try
            {
                using (connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddRange(parameters);
                        await command.ExecuteNonQueryAsync();
                    }

                }
                return true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance.Error("数据库执行报错:" + e.StackTrace);
                return false;
            }
            finally
            {
                try
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                }
                catch (Exception e)
                {
                    SimpleLogger.Instance.Error("数据库连接关闭失败:" + e.StackTrace);
                }
            }
        }


        /// <summary>
        /// 动态参数的sql执行
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>

        protected async Task<QueryResult<T>> DoQuery<T>(string sql) where T : new()
        {
            SQLiteConnection connection = null;
            try
            {
                List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();

                using (connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        DbDataReader reader = await command.ExecuteReaderAsync();

                        List<string> columnNameList = new List<string>();

                        // 获取所有列名
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            columnNameList.Add(reader.GetName(i));
                        }

                        while (reader.Read())
                        {
                            Dictionary<string, object> rowMapping = new Dictionary<string, object>();
                            foreach (string loopName in columnNameList)
                            {
                                rowMapping[loopName] = reader[loopName];
                            }
                            resultList.Add(rowMapping);
                        }

                    }
                }
                QueryResult<T> result = new QueryResult<T>(resultList);

                return result;


            }
            catch (Exception e)
            {
                SimpleLogger.Instance.Error("数据库执行报错:" + e.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// 动态参数的sql执行，示例（sql = "SELECT * FROM Users WHERE Age > @age", parameters = new SQLiteParameter("@age", 18)）
        /// </summary>
        /// <param name="sql">sql, 参数用@name占位</param>
        /// <param name="parameters">示例new SQLiteParameter("@age", 18)</param>
        /// <returns></returns>
        protected async Task<QueryResult<T>> DoQuery<T>(string sql, SQLiteParameter[] parameters) where T : new()
        {
            SQLiteConnection connection = null;
            try
            {
                List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();

                using (connection = new SQLiteConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddRange(parameters);
                        DbDataReader reader = await command.ExecuteReaderAsync();

                        List<string> columnNameList = new List<string>();

                        // 获取所有列名
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            columnNameList.Add(reader.GetName(i));
                        }

                        while (reader.Read())
                        {
                            Dictionary<string, object> rowMapping = new Dictionary<string, object>();
                            foreach (string loopName in columnNameList)
                            {
                                rowMapping[loopName] = reader[loopName];
                            }
                            resultList.Add(rowMapping);
                        }

                    }
                }
                QueryResult<T> result = new QueryResult<T>(resultList);

                return result;

            }
            catch (Exception e)
            {
                SimpleLogger.Instance.Error("数据库执行报错:" + e.StackTrace);
                return null;
            }
        }

    }
}
