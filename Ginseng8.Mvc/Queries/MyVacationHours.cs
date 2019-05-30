using Ginseng.Models;
using Postulate.Base;
using System;

namespace Ginseng.Mvc.Queries
{
    public class MyVacationHours : Query<VacationHours>
    {
        public MyVacationHours() : base(
            @"SELECT [vh].*
            FROM [dbo].[VacationHours] [vh]
            WHERE [OrganizationId]=@orgId AND [UserId]=@userId AND [Date]>=@date")
        {
        }

        public int OrgId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
    }
}