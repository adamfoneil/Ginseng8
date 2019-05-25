using Ginseng.Mvc.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Testing
{
    [TestClass]
    public class AutoPlannerTests
    {
        [TestMethod]
        public void Case1()
        {
            var workHours = new DailyWorkHoursResult[]
            {
                new DailyWorkHoursResult() { Date = new DateTime(2019, 5, 27), Hours = 5 },
                new DailyWorkHoursResult() { Date = new DateTime(2019, 5, 28), Hours = 5 },
                new DailyWorkHoursResult() { Date = new DateTime(2019, 5, 29), Hours = 5 },
                new DailyWorkHoursResult() { Date = new DateTime(2019, 5, 30), Hours = 5 },
                new DailyWorkHoursResult() { Date = new DateTime(2019, 5, 31), Hours = 5 }
            };

            var estimates = new WorkItemEstimateHoursResult[]
            {
                new WorkItemEstimateHoursResult() { Id = 2539, Hours =  3 },
                new WorkItemEstimateHoursResult() { Id = 2528, Hours = 3 },
                new WorkItemEstimateHoursResult() { Id = 2507, Hours = 3 },
                new WorkItemEstimateHoursResult() { Id = 2527, Hours = 8 },
                new WorkItemEstimateHoursResult() { Id = 2536, Hours = 3 }
            };

            /*
            5/27: 2539,3; 2528,2
            5/28: 2528,1; 2507,3; 2527,1;
            5/29: 2527,5;
            5/30: 2527,2; 2536,3
            */
        }
    }
}