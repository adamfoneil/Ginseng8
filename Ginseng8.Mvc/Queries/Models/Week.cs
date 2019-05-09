namespace Ginseng.Mvc.Queries.Models
{
    public class Week
    {
        public int Year { get; set; }
        public int WeekNumber { get; set; }

        public override bool Equals(object obj)
        {
            var test = obj as Week;
            return (test != null) ? test.Year == Year && test.WeekNumber == WeekNumber : false;
        }

        public override int GetHashCode()
        {
            return (Year + WeekNumber).GetHashCode();
        }
    }
}