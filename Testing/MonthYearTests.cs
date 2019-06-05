using Ginseng.Mvc.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing
{
    [TestClass]
    public class MonthYearTests
    {
        [TestMethod]
        public void AddYearMonthsThisYear()
        {
            var current = new YearMonth(2019, 6);
            var add3Months = current += 3;
            Assert.IsTrue(add3Months.Year == 2019 && add3Months.Month == 9);
        }

        [TestMethod]
        public void AddYearsMonthsNextYear()
        {
            var current = new YearMonth(2019, 6);
            var add8Months = current += 8;
            Assert.IsTrue(add8Months.Year == 2020 && add8Months.Month == 2);
        }

        [TestMethod]
        public void AddYearsManyMonths()
        {
            var current = new YearMonth(2019, 6);
            var future = current += 17;
            Assert.IsTrue(future.Year == 2020 && future.Month == 11);
        }
    }
}