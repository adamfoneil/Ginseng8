﻿using Dapper;
using Ginseng.Models.Conventions;
using Ginseng.Models.Extensions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    /// <summary>
    /// A due date of some kind, such as a sprint end date or some other event with a known date
    /// </summary>
    [DereferenceId("SELECT [Name] + ' ' + FORMAT([Date], 'ddd M/d') AS [Name] FROM [dbo].[Milestone] WHERE [Id]=@id")]
    public class Milestone : BaseTable, IOrgSpecific, IFindRelated<int>, IMilestoneHeader
    {
        public Milestone()
        {
        }

        public Milestone(DateTime date)
        {
            Date = date;

            var dtfi = DateTimeFormatInfo.CurrentInfo;
            var cal = dtfi.Calendar;
            int weekNumber = cal.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
            Name = $"week {weekNumber}";
        }

        [PrimaryKey]
        [References(typeof(Organization))]
        public int OrganizationId { get; set; }
        
        [MaxLength(50)]
        [PrimaryKey]
        public string Name { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:M/d/yy}")]
        public DateTime Date { get; set; }

        [References(typeof(Team))]
        public int? TeamId { get; set; }

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

        [NotMapped]
        public int ProjectId { get; set; }

        public Organization Organization { get; set; }

        public static async Task EnsureUniqueNameAsync(IDbConnection cn, Milestone ms)
        {
            int increment = 0;
            string baseName = ms.Name;
            while (await IsDuplicateNameAsync(cn, ms))
            {
                increment++;
                ms.Name = baseName + "." + increment.ToString();
            }
        }

        private static async Task<bool> IsDuplicateNameAsync(IDbConnection cn, Milestone ms)
        {
            return await cn.ExistsWhereAsync<Milestone>(new { ms.OrganizationId, ms.Name });
        }

        public Team Team { get; set; }

        public int MilestoneId => Id;
        public string MilestoneName => Name;
        public DateTime? MilestoneDate => Date;

        [NotMapped]
        public int? MilestoneDaysAway { get; set; }

        [NotMapped]
        public bool IsMilestoneVisible { get; set; }

        public static async Task<Milestone> GetLatestAsync(IDbConnection connection, int teamId)
        {
            if (teamId == 0) return null;

            return await connection.QuerySingleOrDefaultAsync<Milestone>(
                @"WITH [source] AS (
					SELECT MAX([Date]) AS [MaxDate]
					FROM [dbo].[Milestone]
					WHERE [TeamId]=@teamId
				) SELECT TOP (1) [ms].*
				FROM
					[dbo].[Milestone] [ms] INNER JOIN [source] [src] ON [ms].[Date]=[src].[MaxDate]
				WHERE
					[TeamId]=@teamId", new { teamId });
        }

        public static async Task<Milestone> GetSoonestNextAsync(IDbConnection connection, int teamId)
        {
            if (teamId == 0) return null;

            return await connection.QuerySingleOrDefaultAsync<Milestone>(
                @"WITH [source] AS (
					SELECT MIN([Date]) AS [MinDate]
					FROM [dbo].[Milestone]
					WHERE [TeamId]=@teamId AND [Date]>getdate()
				) SELECT TOP (1) [ms].*
				FROM
					[dbo].[Milestone] [ms] INNER JOIN [source] [src] ON [ms].[Date]=[src].[MinDate]
				WHERE
					[TeamId]=@teamId", new { teamId });
        }

        public static async Task<Milestone> CreateNextAsync(IDbConnection connection, int teamId)
        {
            var team = await connection.FindAsync<Team>(teamId);
            var latest = await GetLatestAsync(connection, teamId);

            DayOfWeek nextDayOfWeek = team.Organization.MilestoneWorkDay.ToDayOfWeek();
            DateTime nextDate =
                latest?.Date.NextDayOfWeek(nextDayOfWeek, team.Organization.IterationWeeks) ??
                DateTime.Today.NextDayOfWeek(nextDayOfWeek, team.Organization.IterationWeeks);

            return new Milestone(nextDate)
            {
                TeamId = team.Id,                
                Date = nextDate                
            };
        }

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return await Task.FromResult(OrganizationId);
        }

        public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider, IUser user = null, IEnumerable<Claim> claims = null)
        {
            Organization = commandProvider.Find<Organization>(connection, OrganizationId);
            if (TeamId.HasValue) Team = commandProvider.Find<Team>(connection, TeamId.Value);
        }

        public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider, IUser user = null, IEnumerable<Claim> claims = null)
        {
            Organization = await commandProvider.FindAsync<Organization>(connection, OrganizationId);
            if (TeamId.HasValue) Team = await commandProvider.FindAsync<Team>(connection, TeamId.Value);
        }
    }
}