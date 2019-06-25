using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    public enum HoursSourceType
    {
        Comment = 1,
        CommitMessage = 2
    }

    public class PendingWorkLog : BaseTable, IBody, IOrgSpecific
    {
        [References(typeof(Organization))]
        public int OrganizationId { get; set; }

        [References(typeof(Team))]
        public int TeamId { get; set; }

        /// <summary>
        /// Work must at minimum be related to a project
        /// </summary>
        [References(typeof(Project))]
        public int? ProjectId { get; set; }

        /// <summary>
        /// work hours are usually in reference to a specific work item.
        /// If there's no work item, it means the work relates to project definition/management by itself
        /// </summary>
        [References(typeof(WorkItem))]
        public int? WorkItemId { get; set; }

        [References(typeof(Application))]
        public int? ApplicationId { get; set; }

        [References(typeof(UserProfile))]
        public int UserId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [DecimalPrecision(4, 2)]
        public decimal Hours { get; set; }

        public string TextBody { get; set; }

        public string HtmlBody { get; set; }

        /// <summary>
        /// if the hours came from a comment or commit message, that's indicated here
        /// </summary>
        public HoursSourceType? SourceType { get; set; }

        /// <summary>
        /// Commit message or comment id that this record was generated from
        /// </summary>
        public int? SourceId { get; set; }

        [NotMapped]
        public int? WorkItemNumber { get; set; }

        [NotMapped]
        public string WorkItemTitle { get; set; }

        public override bool Validate(IDbConnection connection, out string message)
        {
            if (!ProjectId.HasValue && !WorkItemId.HasValue)
            {
                message = "Work logs must specify the project or work item.";
                return false;
            }

            message = null;
            return true;
        }

        public static async Task FromCommentAsync(IDbConnection connection, Comment comment, IUser user)
        {
            if (comment.ObjectType != ObjectType.WorkItem && comment.ObjectType != ObjectType.Project) return;
            var currentUser = user as UserProfile;
            if (currentUser == null) throw new Exception("Couldn't determine the current user profile when creating work log.");

            if (ParseHoursFromText(comment.TextBody, out decimal hours))
            {
                var link = await FindProjectAndWorkItemIdAsync(connection, comment);

                var workLog = new PendingWorkLog()
                {
                    OrganizationId = link.OrganizationId,
                    TeamId = link.TeamId,
                    ApplicationId = link.ApplicationId,
                    ProjectId = link.ProjectId,
                    WorkItemId = link.WorkItemId,
                    UserId = currentUser.UserId,
                    Date = comment.DateCreated,
                    Hours = hours,
                    TextBody = comment.TextBody,
                    HtmlBody = comment.HtmlBody,
                    SourceType = HoursSourceType.Comment,
                    SourceId = comment.Id
                };

                await connection.SaveAsync(workLog, user ?? new SystemUser() { UserName = "system", LocalTime = DateTime.UtcNow });
            }
        }

        private static async Task<PendingHoursLink> FindProjectAndWorkItemIdAsync(IDbConnection connection, Comment comment)
        {
            switch (comment.ObjectType)
            {
                case ObjectType.Project:
                    var prj = await connection.FindAsync<Project>(comment.ObjectId);
                    return new PendingHoursLink()
                    {
                        TeamId = prj.TeamId,
                        OrganizationId = prj.Application.OrganizationId,
                        ApplicationId = prj.ApplicationId,
                        ProjectId = comment.ObjectId,
                        WorkItemId = null
                    };

                case ObjectType.WorkItem:
                    var workItem = await connection.FindAsync<WorkItem>(comment.ObjectId);
                    return new PendingHoursLink()
                    {
                        TeamId = workItem.TeamId,
                        OrganizationId = workItem.OrganizationId,
                        ApplicationId = workItem.ApplicationId,
                        ProjectId = workItem.ProjectId,
                        WorkItemId = workItem.Id
                    };
            }

            throw new ArgumentException($"Comment object type {comment.ObjectType} does not support hours reporting");
        }

        public static bool ParseHoursFromText(string input, out decimal hours)
        {
            var match = Regex.Match(input, @"\+(\d*(\.[0-9][0-9]?)?)");

            if (match.Success)
            {
                hours = decimal.Parse(match.Value.Substring(1));
                return true;
            }

            hours = 0;
            return false;
        }

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return await Task.FromResult(OrganizationId);
        }
    }

    internal class PendingHoursLink
    {
        public int OrganizationId { get; set; }
        public int TeamId { get; set; }
        public int? ApplicationId { get; set; }
        public int? ProjectId { get; set; }
        public int? WorkItemId { get; set; }
    }
}