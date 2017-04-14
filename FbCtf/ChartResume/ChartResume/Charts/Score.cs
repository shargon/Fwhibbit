using ChartResume.Helpers;
using System;

namespace ChartResume.Charts
{
    public class Score
    {
        public int Index = 0;
        public int Points = 0;
        public DateTime Date;
        public string ChangeDay = "";

        public Score(int points, DateTime date)
        {
            Points = points;
            Date = date;
        }
        public string ToString(int max)
        {
            return MathHelper.Percentage(max, Points).ToString("0");
        }
        public override string ToString()
        {
            return Points.ToString() + "-" + Date.ToString();
        }
    }
}