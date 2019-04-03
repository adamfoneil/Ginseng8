using System;

namespace Ginseng.Models.Extensions
{
	public static class DateExtensions
	{
		public static DateTime NextDayOfWeek(this DateTime date, DayOfWeek dayOfWeek, int weekOffset = 0)
		{
			DayOfWeek thisDayOfWeek = date.DayOfWeek;
			int addDays = dayOfWeek - thisDayOfWeek;
			int thisWeekOffset = 0;
			if (addDays < 0) thisWeekOffset = 7;
			return date.AddDays(addDays + thisWeekOffset + (Math.Abs(weekOffset) * 7));
		}
	}
}