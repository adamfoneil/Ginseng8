using Dapper;
using Ginseng.Models.Conventions;
using Ginseng.Models.Extensions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using Postulate.SqlServer.IntKey;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	/// <summary>
	/// A due date of some kind, such as a sprint end date or some other event with a known date
	/// </summary>
	[DereferenceId("SELECT [Name] + ' ' + FORMAT([Date], 'ddd M/d') AS [Name] FROM [dbo].[Milestone] WHERE [Id]=@id")]
	public class Milestone : BaseTable, IOrgSpecific
	{
		[PrimaryKey]
		[References(typeof(Organization))]
		public int OrganizationId { get; set; }

		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }

		[Column(TypeName = "date")]
		[DisplayFormat(DataFormatString = "{0:M/d/yy}")]
		public DateTime Date { get; set; }

		[NotMapped]
		public int? DaysAway { get; set; }

		/// <summary>
		/// This is false in case where we're using a placeholder milestone for crosstab display purposes
		/// </summary>
		[NotMapped]
		public bool ShowDate { get; set; } = true;

		public static async Task<Milestone> GetLatestAsync(IDbConnection connection, int orgId)
		{
			return await connection.QuerySingleOrDefaultAsync<Milestone>(
				@"WITH [source] AS (
					SELECT MAX([Date]) AS [MaxDate]
					FROM [dbo].[Milestone]
					WHERE [OrganizationId]=@orgId
				) SELECT [ms].*
				FROM
					[dbo].[Milestone] [ms] INNER JOIN [source] [src] ON [ms].[Date]=[src].[MaxDate]
				WHERE
					[OrganizationId]=@orgId", new { orgId });
		}

		public static async Task<Milestone> GetSoonestNextAsync(IDbConnection connection, int orgId)
		{
			return await connection.QuerySingleOrDefaultAsync<Milestone>(
				@"WITH [source] AS (
					SELECT MIN([Date]) AS [MinDate]
					FROM [dbo].[Milestone]
					WHERE [OrganizationId]=@orgId AND [Date]>getdate()
				) SELECT [ms].*
				FROM 
					[dbo].[Milestone] [ms] INNER JOIN [source] [src] ON [ms].[Date]=[src].[MinDate]
				WHERE
					[OrganizationId]=@orgId", new { orgId });
		}

		public static async Task<Milestone> CreateNextAsync(IDbConnection connection, int orgId)
		{
			var org = await connection.FindAsync<Organization>(orgId);
			var latest = await GetLatestAsync(connection, orgId);
			
			DayOfWeek nextDayOfWeek = org.MilestoneWorkDay.ToDayOfWeek();
			DateTime nextDate = 
				latest?.Date.NextDayOfWeek(nextDayOfWeek, org.IterationWeeks) ?? 
				DateTime.Today.NextDayOfWeek(nextDayOfWeek, org.IterationWeeks);

			DateTimeFormatInfo dtfi = DateTimeFormatInfo.CurrentInfo;
			var cal = dtfi.Calendar;
			int weekNumber = cal.GetWeekOfYear(nextDate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

			return new Milestone()
			{
				OrganizationId = orgId,
				Date = nextDate,
				Name = $"week {weekNumber}"
			};
		}

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return await Task.FromResult(OrganizationId);
        }
    }
}