using System;

namespace Ginseng.Mvc.Classes
{    
    public class YearMonth
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public YearMonth()
        {
        }

        public YearMonth(DateTime dateTime)
        {
            Year = dateTime.Year;
            Month = dateTime.Month;
        }

        public YearMonth(int year, int month)
        {
            Year = year;
            Month = month;
        }

        public DateTime EndDate()
        {
            // thanks to https://stackoverflow.com/a/4655207/2023653
            return new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));
        }

        public override string ToString()
        {
            DateTime date = new DateTime(Year, Month, 1);
            return date.ToString("MMMM yyyy");
        }

        public string ToString(string format)
        {
            DateTime date = new DateTime(Year, Month, 1);
            return date.ToString(format);
        }

        public static YearMonth operator ++(YearMonth value)
        {
            return value + 1;
        }

        public static YearMonth operator +(YearMonth value, int months)
        {
            int totalMonths = months + value.Month;
            int years = totalMonths / 12;
            int leftover = totalMonths % 12;
            return new YearMonth(value.Year + years, leftover);
        }
    }
}