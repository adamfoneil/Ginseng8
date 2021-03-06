﻿using System;

namespace Ginseng.Mvc.Classes
{
    public class YearMonth : IComparable<YearMonth>
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Index { get; set; }

        public YearMonth() : this(DateTime.Today)
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
            int year = value.Year;
            var result = new YearMonth(value.EndDate().AddMonths(months)) { Index = value.Index + 1 };
            if (result.Year > year) result.Index = 0;
            return result;
        }

        public static YearMonth operator -(YearMonth value, int months)
        {
            return new YearMonth(value.EndDate().AddMonths(months * -1));
        }

        public static bool operator <(YearMonth value1, YearMonth value2)
        {
            return value1.EndDate() < value2.EndDate();
        }

        public static bool operator >(YearMonth value1, YearMonth value2)
        {
            return value1.EndDate() > value2.EndDate();
        }

        public static bool operator <=(YearMonth value1, YearMonth value2)
        {
            return value1 < value2 || value1.Equals(value2);
        }

        public static bool operator >=(YearMonth value1, YearMonth value2)
        {
            return value1 > value2 || value1.Equals(value2);
        }

        public override bool Equals(object obj)
        {
            var test = obj as YearMonth;
            return (test != null) ? test.Year == Year && test.Month == Month : false;
        }

        public override int GetHashCode()
        {
            return (Year + Month).GetHashCode();
        }

        public int CompareTo(YearMonth other)
        {
            return EndDate().CompareTo(other);
        }
    }
}