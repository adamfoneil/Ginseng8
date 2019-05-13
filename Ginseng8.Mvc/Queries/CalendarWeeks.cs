using Ginseng.Mvc.Queries.Models;
using Postulate.Base;
using Postulate.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Ginseng.Mvc.Queries
{
	public class CalendarWeeksResult
	{
		public int WeekIndex { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Year { get; set; }
		public int WeekNumber { get; set; }

        public Week ToWeek() { return new Week() { Year = Year, WeekNumber = WeekNumber }; }
	}

	/// <summary>
	/// Gives you start/end dates of weeks counting backwards from the current week
	/// </summary>
	public class CalendarWeeks : Query<CalendarWeeksResult>, ITestableQuery
	{
		public CalendarWeeks() : base(
			@"WITH [source] AS (
				SELECT
					[Value]*-1 AS [WeekIndex],
					CONVERT(date, DATEADD(ww, [Value]*-1, @seed)) AS [Date],
					((DATEPART(dw, DATEADD(ww, [Value]*-1, @seed))-1)*-1) AS [Offset]
				FROM
				dbo.FnIntRange(0, @weeksBack)
			) SELECT
				ABS([WeekIndex]) AS [WeekIndex],
				DATEADD(d, [Offset], [Date]) AS [StartDate],
				DATEADD(d, 6, DATEADD(d, [Offset], [Date])) AS [EndDate],
				YEAR(DATEADD(d, [Offset], [Date])) AS [Year],
				DATEPART(ww, DATEADD(d, [Offset], [Date])) AS [WeekNumber]
			FROM
				[source]")
		{
		}

		public DateTime Seed { get; set; }
		public int WeeksBack { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new CalendarWeeks() { Seed = DateTime.Today, WeeksBack = 1 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}