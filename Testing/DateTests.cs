using Ginseng.Models.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Testing
{
	[TestClass]
	public class DateTests
	{
		[TestMethod]
		public void FutureDateNextWeek()
		{
			var today = new DateTime(2019, 4, 3); // a wednesday
			var nextMonday = today.NextDayOfWeek(DayOfWeek.Monday);
			Assert.IsTrue(nextMonday.Equals(new DateTime(2019, 4, 8)));
		}

		[TestMethod]
		public void FutureDateThisWeek()
		{
			var today = new DateTime(2019, 4, 3); // a wednesday
			var nextFriday = today.NextDayOfWeek(DayOfWeek.Friday);
			Assert.IsTrue(nextFriday.Equals(new DateTime(2019, 4, 5)));
		}

		[TestMethod]
		public void FutureDateWithWeekOffset()
		{
			var today = new DateTime(2019, 4, 3); // a wednesday
			var twoThursdaysAway = today.NextDayOfWeek(DayOfWeek.Thursday, 2);
			Assert.IsTrue(twoThursdaysAway.Equals(new DateTime(2019, 4, 18)));
		}

		[TestMethod]
		public void FutureDateWithWeekOffsetBeforeThisWeekday()
		{
			var today = new DateTime(2019, 4, 3); // a wednesday
			var threeMondaysAway = today.NextDayOfWeek(DayOfWeek.Monday, 3);
			Assert.IsTrue(threeMondaysAway.Equals(new DateTime(2019, 4, 22)));
		}
	}
}