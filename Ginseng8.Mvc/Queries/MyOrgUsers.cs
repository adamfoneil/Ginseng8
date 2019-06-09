using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Classes;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
	public class MyOrgUsers : Query<OrganizationUser>, ITestableQuery
	{
        private readonly List<QueryTrace> _traces;

        public MyOrgUsers() : base(
            @"WITH [hours] AS (
                SELECT [UserId], SUM([Hours]) AS [TotalHours]
                FROM [dbo].[FnUserWorkDays](@hoursOrgId)
                GROUP BY [UserId]
            ) SELECT 
				[ou].*,
				[org].[Name] AS [OrgName],
				COALESCE([ou].[DisplayName], [u].[UserName]) AS [UserName],
				CASE
					WHEN [ou].[DisplayName] IS NOT NULL THEN [u].[UserName]
					ELSE NULL
				END AS [Email],                
                [h].[TotalHours] AS [WeeklyHours]
			FROM 
				[dbo].[OrganizationUser] [ou]
				INNER JOIN [dbo].[Organization] [org] ON [ou].[OrganizationId]=[org].[Id]
				INNER JOIN [dbo].[AspNetUsers] [u] ON [ou].[UserId]=[u].[UserId]
                LEFT JOIN [hours] [h] ON [ou].[UserId]=[h].[UserId]
			{where}")
		{
		}

        public MyOrgUsers(List<QueryTrace> traces = null) : this()
        {
            _traces = traces;
        }

		[Case(true, "[org].[OwnerUserId]<>[ou].[UserId]")]
		public bool? ExcludeOwner { get; set; }

		[Where("[ou].[UserId]=@userId")]
		public int? UserId { get; set; }

		[Where("[ou].[OrganizationId]=@orgId")]
		public int? OrgId { get; set; }

        public int HoursOrgId { get; set; } // works around this issue https://github.com/adamosoftware/Postulate/issues/22

        [Where("[ou].[IsRequest]=@isRequest")]
		public bool? IsRequest { get; set; }		

		[Where("[ou].[IsEnabled]=@isEnabled")]
		public bool? IsEnabled { get; set; }

		[Where("[ou].[UserId]<>@excludeUserId")]
		public int? ExcludeUserId { get; set; }

        protected override async Task OnQueryExecutedAsync(QueryTrace queryTrace)
        {
            _traces.Add(queryTrace);
            await Task.CompletedTask;
        }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new MyOrgUsers() { OrgId = 0 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}