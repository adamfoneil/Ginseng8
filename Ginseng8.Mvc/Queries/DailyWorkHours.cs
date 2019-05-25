using Postulate.Base;
using Postulate.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
    public class DailyWorkHoursResult
    {
        public DateTime Date { get; set; }
        public int Hours { get; set; }
    }

    public class DailyWorkHours : Query<DailyWorkHoursResult>, ITestableQuery
    {
        public DailyWorkHours() : base(
            @"SELECT
                [wd].[Date], [wd].[Hours]
            FROM
                [dbo].[FnWorkingDays](@orgId, @startDate, @endDate) [wd]
            WHERE
                [UserId]=@userId")
        {
        }

        public int OrgId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new DailyWorkHours() { OrgId = 0, UserId = 0, StartDate = new DateTime(2019, 5, 25), EndDate = new DateTime(2019, 5, 31) };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}