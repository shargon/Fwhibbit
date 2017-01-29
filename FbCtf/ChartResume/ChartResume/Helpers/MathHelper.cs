namespace ChartResume.Helpers
{
    public class MathHelper
    {
        public static double Percentage(double max, double points)
        {
            if (points == 0) return 0;
            return ((points * 100.0) / max);
        }
    }
}