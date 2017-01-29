using ChartResume.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ChartResume.Charts
{
    public class Chart2
    {
        public static string GetChart(int userId, MySqlCommand com)
        {
            com.Parameters.Clear();
            com.Parameters.AddWithValue("i", userId);

            List<double[]> url2_data = new List<double[]>();
            List<string> url2_cate = new List<string>();

            using (DataTable dtc2 = SqlHelper.SelectDataTable(com, "SELECT flag_sile_nohint, flag_sile_sihint,flag_nole_sihint,flag_nole_nohint,category from vchart_2 where user_id= @i"))
            {
                foreach (DataRow dr2 in dtc2.Rows)
                    url2_cate.Add(dr2["category"].ToString());

                url2_data.Add(new double[dtc2.Rows.Count]);
                url2_data.Add(new double[dtc2.Rows.Count]);
                url2_data.Add(new double[dtc2.Rows.Count]);
                url2_data.Add(new double[dtc2.Rows.Count]);

                for (int x = 0; x < 4; x++)
                {
                    for (int irow = 0; irow < dtc2.Rows.Count; irow++)
                        //for (int irow = dtc2.Rows.Count - 1; irow >= 0; irow--)
                        url2_data[x][irow] = Convert.ToInt32(dtc2.Rows[irow][x]);
                }
            }

            for (int x = 0; x < url2_cate.Count; x++)
            {
                double max = url2_data[0][x] + url2_data[1][x] + url2_data[2][x] + url2_data[3][x];

                for (int xx = 0; xx < 4; xx++)
                    url2_data[xx][x] = MathHelper.Percentage(max, url2_data[xx][x]);
            }

            url2_cate.Reverse();

            return "https://chart.googleapis.com/chart?cht=bhs&chco=82CFF6,3C89B0,A52600,ffa500&chs=780x260&chd=t:" +
                 string.Join("|", url2_data.Select(u => string.Join(",", u.Select(ux => ux.ToString("0"))))) +
                "&chxt=x,y&chxl=1:|" + string.Join("|", url2_cate) +
                "&chdl=Flags%20captured%20without%20hints%7CFlags%20captured%20with%20hints%7CFlags%20uncaptured%20with%20hints%7CFlags%20uncaptured%20without%20hints";
        }
    }
}