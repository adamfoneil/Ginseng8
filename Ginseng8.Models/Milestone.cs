using Dapper;
using Ginseng.Models.Conventions;
using Ginseng.Models.Extensions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
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
	public class Milestone : BaseTable, IOrgSpecific, IFindRelated<int>
	{
        [PrimaryKey]
        [References(typeof(Application))]
        public int ApplicationId { get; set; }

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

        [NotMapped]
        public int? TotalWorkItems { get; set; }

        [NotMapped]
        public int? OpenWorkItems { get; set; }

        [NotMapped]
        public int? ClosedWorkItems { get; set; }

        [NotMapped]
        public string ApplicationName { get; set; }

        public Application Application { get; set; }

		public static async Task<Milestone> GetLatestAsync(IDbConnection connection, int appId)
		{
            if (appId == 0) return null;

			return await connection.QuerySingleOrDefaultAsync<Milestone>(
				@"WITH [source] AS (
					SELECT MAX([Date]) AS [MaxDate]
					FROM [dbo].[Milestone]
					WHERE [ApplicationId]=@appId
				) SELECT TOP (1) [ms].*
				FROM
					[dbo].[Milestone] [ms] INNER JOIN [source] [src] ON [ms].[Date]=[src].[MaxDate]
				WHERE
					[ApplicationId]=@appId", new { appId });
		}

		public static async Task<Milestone> GetSoonestNextAsync(IDbConnection connection, int appId)
		{
            if (appId == 0) return null;

			return await connection.QuerySingleOrDefaultAsync<Milestone>(
				@"WITH [source] AS (
					SELECT MIN([Date]) AS [MinDate]
					FROM [dbo].[Milestone]
					WHERE [ApplicationId]=@appId AND [Date]>getdate()
				) SELECT TOP (1) [ms].*
				FROM 
					[dbo].[Milestone] [ms] INNER JOIN [source] [src] ON [ms].[Date]=[src].[MinDate]
				WHERE
					[ApplicationId]=@appId", new { appId });
		}

		public static async Task<Milestone> CreateNextAsync(IDbConnection connection, int appId)
		{
            var app = await connection.FindAsync<Application>(appId);
			var latest = await GetLatestAsync(connection, appId);
			
			DayOfWeek nextDayOfWeek = app.Organization.MilestoneWorkDay.ToDayOfWeek();
			DateTime nextDate = 
				latest?.Date.NextDayOfWeek(nextDayOfWeek, app.Organization.IterationWeeks) ?? 
				DateTime.Today.NextDayOfWeek(nextDayOfWeek, app.Organization.IterationWeeks);

			DateTimeFormatInfo dtfi = DateTimeFormatInfo.CurrentInfo;
			var cal = dtfi.Calendar;
			int weekNumber = cal.GetWeekOfYear(nextDate, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

			return new Milestone()
			{
				ApplicationId = appId,
				Date = nextDate,
				Name = $"week {weekNumber}"
			};
		}

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            var app = await connection.FindAsync<Application>(ApplicationId);
            return app.OrganizationId;
        }

        public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider)
        {
            Application = commandProvider.Find<Application>(connection, ApplicationId);
        }

        public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider)
        {
            Application = await commandProvider.FindAsync<Application>(connection, ApplicationId);
        }
    }
}