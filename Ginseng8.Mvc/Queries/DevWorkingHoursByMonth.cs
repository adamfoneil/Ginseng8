using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
    public class DevWorkingHoursByMonthResult
    {
        public string UserName { get; set; }
        public int UserId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int WorkingHours { get; set; }
    }

    public class DevWorkingHoursByMonth : Query<DevWorkingHoursByMonthResult>, ITestableQuery
    {
        public DevWorkingHoursByMonth() : base(
            @"SELECT
                COALESCE([ou].[DisplayName], [u].[UserName]) AS [UserName],
                [wd].[UserId],
                YEAR([wd].[Date]) AS [Year],
                MONTH([wd].[Date]) AS [Month],
                SUM([Hours]) AS [WorkingHours]
            FROM
                [dbo].[FnWorkingDays](@orgId, getdate(), @endDate) [wd]
                INNER JOIN [dbo].[OrganizationUser] [ou] ON [wd].[UserId]=[ou].[UserId] AND [ou].[OrganizationId]=@orgId
                INNER JOIN [dbo].[AspNetUsers] [u] ON [ou].[UserId]=[u].[UserId]
            {where}
            GROUP BY
                COALESCE([ou].[DisplayName], [u].[UserName]),
                [wd].[UserId],
                YEAR([wd].[Date]),
                MONTH([wd].[Date])")
        {
        }

        public int OrgId { get; set; }
        public DateTime EndDate { get; set; }

        [Where("[wd].[UserId] IN @userIds")]
        public int[] UserIds { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new DevWorkingHoursByMonth() { OrgId = 1, EndDate = new DateTime(2019, 12, 31) };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}