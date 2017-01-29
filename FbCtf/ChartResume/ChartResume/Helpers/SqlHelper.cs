using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace ChartResume.Helpers
{
    public class SqlHelper
    {
        /// <summary>
        /// Select DataTable
        /// </summary>
        /// <param name="com">Command</param>
        /// <param name="sql">Sql</param>
        public static DataTable SelectDataTable(DbCommand com, string sql)
        {
            com.CommandText = sql;
            com.CommandType = CommandType.Text;

            using (DbDataAdapter adapter = DbProviderFactories.GetFactory(com.Connection).CreateDataAdapter())
            {
                adapter.SelectCommand = com;

                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
        /// <summary>
        /// Select Row
        /// </summary>
        /// <param name="com">Command</param>
        /// <param name="sql">Sql</param>
        /// <param name="row">Row</param>
        public static bool SelectRow(MySqlCommand com, string sql, object[] row)
        {
            com.CommandText = sql;
            com.CommandType = CommandType.Text;

            using (MySqlDataReader reader = com.ExecuteReader())
            {
                if (reader.Read())
                {
                    reader.GetValues(row);
                    return true;
                }
            }

            return false;
        }
    }
}