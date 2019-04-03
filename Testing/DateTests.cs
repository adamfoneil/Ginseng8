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
	}
}