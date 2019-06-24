using System;

namespace Ginseng.Models.Extensions
{
    public static class DateExtensions
    {
        public static DateTime NextDayOfWeek(this DateTime date, DayOfWeek dayOfWeek, int weekOffset = 0)
        {
            DayOfWeek thisDayOfWeek = date.DayOfWeek;
            int addDays = dayOfWeek - thisDayOfWeek;
            if (addDays < 0 && weekOffset == 0) weekOffset++; // go to next week if you asked for this week, but the day you want has passed
            return date.AddDays(addDays + (Math.Abs(weekOffset) * 7));
        }

        public static DateTime EndOfMonth(DateTime today)
        {
            int year = today.Year;
            int month = today.Month;
            return new DateTime(year, month, DateTime.DaysInMonth(year, month));
        }
    }
}