using Dapper.QX;
using System;

namespace Ginseng.Mvc.Queries
{
    public class UserAwayDaysResult
    {
        public int UserId { get; set; }
        public int TotalHours { get; set; }
    }

    public class UserAwayDays : Query<UserAwayDaysResult>
    {
        public UserAwayDays() : base(
            @"SELECT 
                [UserId],
                SUM([Hours]) AS [TotalHours]
            FROM
                [dbo].[VacationHours] [vh]
            WHERE
                [vh].[OrganizationId]=@orgId AND
                [vh].[Date] BETWEEN @startDate AND @endDate
            GROUP BY
                [UserId]")
        {
        }

        public int OrgId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

    }

}
