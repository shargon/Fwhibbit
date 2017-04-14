using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ChartResume.Charts
{
    public class Chart3
    {
        public static string GetChart(int userId, string user, MySqlCommand com, DataTable scores, Score[] global, int max)
        {
            max += 500;
            List<Score> lg = new List<Score>(global);
            List<Score> lc= new List<Score>(GetUserScore(userId, scores));

            int lgg = -1, lcc = -1;
            for (int x = 0; x < lg.Count; x++)
            {
                if (lgg == lg[x].Points && lcc == lc[x].Points)
                {
                    lg.RemoveAt(x);
                    lc.RemoveAt(x);
                    x--;
                    continue;
                }

                lgg = lg[x].Points;
                lcc = lc[x].Points;
            }

            string total = string.Join(",", lg.Select(u => u.ToString(max)));
            string tiene = string.Join(",", lc.Select(u => u.ToString(max)));

            return "https://chart.googleapis.com/chart?cht=lc&chs=780x280&chco=3D7930,ff0000&chls=2%7C2,10&chg=14.3,-1,1,1&chxt=y&chxr=0,0," + max.ToString() + "&chd=t:" +
                tiene + "|" + total
                + "&chm=B,C5D4B5BB,0,0,0|" + string.Join("|",
                global.Where(u => !string.IsNullOrEmpty(u.ChangeDay)).Select(u => "A" + u.ChangeDay + ",666666,0," + u.Index + ",15")
                ) + "&chdl=You|Total";
        }

        static public int WhoWon(Dictionary<int, int> players, out int userMax)
        {
            userMax = -1;
            int scoreMax = -1;

            foreach (KeyValuePair<int, int> k in players)
                if (userMax == -1 || scoreMax < k.Value)
                {
                    userMax = k.Key;
                    scoreMax = k.Value;
                }

            return scoreMax;
        }
        static IEnumerable<Score> GetUserScore(int userId, DataTable scores)
        {
            Dictionary<int, int> players = new Dictionary<int, int>();

            int lastUserScore = 0;
            foreach (DataRow dr in scores.Rows)
            {
                int cid = Convert.ToInt32(dr["user_id"]);
                int points = Convert.ToInt32(dr["points"]);

                if (players.ContainsKey(cid)) players[cid] += points;
                else players.Add(cid, points);

                int score;
                if (userId != -1)
                {
                    if (cid == userId) lastUserScore += points;
                    score = lastUserScore;
                }
                else
                {
                    int uid;
                    score = WhoWon(players, out uid);
                }

                yield return new Score(score, (DateTime)dr["ts"]);
            }
        }
        public static Score[] Preload(DataTable dtscores, out int maxScore)
        {
            Score[] globalScore = GetUserScore(-1, dtscores).ToArray();

            DateTime date = DateTime.MinValue;

            int ix = 0;
            int days = 0;
            foreach (Score s in globalScore)
            {
                s.Index = ix;
                if (date == DateTime.MinValue || date.ToString("yyyy-MM-dd") != s.Date.ToString("yyyy-MM-dd"))
                {
                    date = s.Date;

                    days++;
                    if (days == 1) s.ChangeDay = "Start";
                    else s.ChangeDay = "Day " + days.ToString();
                }
                ix++;
            }
            maxScore = globalScore.Last().Points;
            globalScore.Last().ChangeDay = "End";

            return globalScore;
        }
    }
}