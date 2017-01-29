using ChartResume.Helpers;
using MySql.Data.MySqlClient;

namespace ChartResume.Charts
{
    public class Chart1
    {
        public static string GetChart(int userId, MySqlCommand com)
        {
            object[] row = new object[4];
            com.Parameters.Clear();
            com.Parameters.AddWithValue("i", userId);
            if (!SqlHelper.SelectRow(com, "SELECT flag_sile_nohint, flag_sile_sihint,flag_nole_sihint,flag_nole_nohint from vchart_1 where user_id= @i", row)) return null;

            string url1 = "https://chart.googleapis.com/chart?cht=p&chd=t:{1},{2},{3},{4}&chs=780x180&chl=Flags%20captured%20without%20hints%7CFlags%20captured%20with%20hints%7CFlags%20uncaptured%20with%20hints%7CFlags%20uncaptured%20without%20hints&chco=82CFF6%7C3C89B0%7CA52600%7Cffa500&chdl={1}|{2}|{3}|{4}";

            for (int x = 0; x < 4; x++) url1 = url1.Replace("{" + (x + 1).ToString() + "}", row[x].ToString());

            return url1;
        }
    }
}