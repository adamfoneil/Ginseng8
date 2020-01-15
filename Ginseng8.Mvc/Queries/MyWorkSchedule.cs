using Dapper.QX;
using Dapper.QX.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
    public class MyWorkScheduleResult
    {
        public int Number { get; set; }
        public DateTime Date { get; set; }
        public int Hours { get; set; }
        public int Priority { get; set; }
    }

    public class MyWorkSchedule : Query<MyWorkScheduleResult>, ITestableQuery
    {
        public MyWorkSchedule() : base("SELECT * FROM [dbo].[FnWorkItemSchedule](@orgId, @userId) ORDER BY [Date], [Priority]")
        {
        }

        public int OrgId { get; set; }
        public int UserId { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new MyWorkSchedule() { OrgId = 0, UserId = 0 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}