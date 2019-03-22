using Postulate.Base;
using System;

namespace Ginseng.Mvc.Queries
{
	public class WorkDaysResult
	{
		public int UserId { get; set; }
		public DateTime Date { get; set; }
		public int Hours { get; set; }
		public int DayOfWeek { get; set; }
		public int Flag { get; set; }
		public int WeekNumber { get; set; }
		public int DayNumber { get; set; }
		public int? TotalHours { get; set; }
	}

	public class WorkDays : Query<WorkDaysResult>
	{
		public WorkDays() : base(
			@"SELECT 
				[wd].*,
				SUM([Hours]) OVER (PARTITION BY [UserId] ORDER BY [Date]) AS [TotalHours]
			FROM 
				[dbo].[FnWorkingDays](@orgId, @startDate, @endDate) 
			{where}")
		{
		}

		public int OrgId { get; set; }
		public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-1);
		public DateTime EndDate { get; set; } = DateTime.Today.AddDays(45);
	}
}