using Dapper;
using Ginseng.Models.Conventions;
using Ginseng.Models.Extensions;
using Ginseng.Models.Interfaces;
using Ginseng.Models.Internal;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.Base.Models;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    /// <summary>
    /// User story, development task, bug, or feature request
    /// </summary>
    [TrackChanges(IgnoreProperties = "ModifiedBy;DateModified")]
    public class WorkItem : BaseTable, IBody, ITrackedRecord, IOrgSpecific, IFindRelated<int>
    {
        public const string IconCreated = "far fa-plus-hexagon";
        public const string IconClosed = "far fa-clipboard-check";

        [References(typeof(Organization))]
        [PrimaryKey]
        [ColumnAccess(SaveAction.Insert)]
        public int OrganizationId { get; set; }

        [PrimaryKey]
        [ColumnAccess(SaveAction.Insert)]
        public int Number { get; set; }

        [MaxLength(255)]
        [Required]
        public string Title { get; set; }

        [References(typeof(UserProfile))]
        public int? BusinessUserId { get; set; }

        [References(typeof(UserProfile))]
        public int? DeveloperUserId { get; set; }

        [References(typeof(HandOff))]
        public int? LastHandOffId { get; set; }

        public string TextBody { get; set; }

        public string HtmlBody { get; set; }

        /// <summary>
        /// Indicates that a comment was added with IsImpediment = true
        /// </summary>
        [DefaultExpression("0")]
        public bool HasImpediment { get; set; }

        [References(typeof(Team))]
        public int TeamId { get; set; }

        [References(typeof(Application))]
        public int? ApplicationId { get; set; }

        /// <summary>
        /// Broad estimate of amount of work involved
        /// </summary>
        [References(typeof(WorkItemSize))]
        public int? SizeId { get; set; }

        /// <summary>
        /// What should be happening with this item?
        /// </summary>
        [References(typeof(Activity))]
        public int? ActivityId { get; set; }

        [References(typeof(Project))]
        public int? ProjectId { get; set; }

        [References(typeof(Milestone))]
        public int? MilestoneId { get; set; }

        [References(typeof(CloseReason))]
        public int? CloseReasonId { get; set; }

        public bool UseDefaultHistoryTable => true;

        public WorkItemTicket WorkItemTicket { get; set; }

        public Organization Organization { get; set; }

        /// <summary>
        /// Default label to create with item
        /// </summary>
        [NotMapped]
        public int LabelId { get; set; }

        public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action, IUser user)
        {
            if (action == SaveAction.Insert)
            {
                if (LabelId != 0)
                {
                    var wil = new WorkItemLabel() { WorkItemId = Id, LabelId = LabelId };
                    await connection.SaveAsync(wil, user);
                }

                await ParseLabelsAsync(connection);
                await ParseProjectAsync(connection);

                await EventLog.WriteAsync(connection, new EventLog()
                {
                    WorkItemId = Id,
                    OrganizationId = OrganizationId,
                    TeamId = TeamId,
                    ApplicationId = ApplicationId,
                    EventId = SystemEvent.WorkItemCreated,
                    HtmlBody = Title,
                    TextBody = Title,
                    IconClass = IconCreated
                }, user);
            }

            await ClosePlaceholderItemsAsync(connection, OrganizationId, MilestoneId, user);
        }

        private async Task ParseProjectAsync(IDbConnection connection)
        {
            var projectMatch = Regex.Match(Title, @"\[.*\]");
            if (projectMatch.Success)
            {
                string projectToken = projectMatch.Value.Substring(1);
                projectToken = projectToken.Substring(0, projectToken.Length - 1);

                var projectId = await connection.QuerySingleOrDefaultAsync<int>(
                    @"SELECT TOP (1) [Id] FROM [dbo].[Project]
					WHERE [ApplicationId]=@appId AND
					([Name] LIKE '%' + @projectName + '%' OR [Nickname]=@projectName)", new { appId = ApplicationId, projectName = projectToken });

                if (projectId != 0)
                {
                    ProjectId = projectId;
                    await connection.UpdateAsync(this, null, r => r.ProjectId);
                }
            }
        }

        private async Task ParseLabelsAsync(IDbConnection connection)
        {
            var labelMatches = Regex.Matches(Title, @"#\w*");
            var labelNames = labelMatches.Cast<Match>().Select(m => m.Value.Substring(1)).ToArray();

            if (labelNames.Any())
            {
                await connection.ExecuteAsync(
                    @"INSERT INTO [dbo].[WorkItemLabel] (
						[WorkItemId], [LabelId], [CreatedBy], [DateCreated]
					) SELECT
						@workItemId, [Id], @userName, @dateCreated
					FROM
						[dbo].[Label]
					WHERE
						[Name] IN @labelNames AND
						[OrganizationId]=@orgId",
                    new { workItemId = Id, orgId = OrganizationId, labelNames, userName = CreatedBy, dateCreated = DateCreated });
            }
        }

        public async Task SetNumberAsync(IDbConnection connection)
        {
            if (Number == 0 && Id == 0)
            {
                int result = await connection.QuerySingleAsync<int>(
                    @"SELECT [NextWorkItemNumber] FROM [dbo].[Organization] WHERE [Id]=@orgId;
					UPDATE [dbo].[Organization] SET [NextWorkItemNumber]=[NextWorkItemNumber]+1 WHERE [Id]=@orgId", new { orgId = OrganizationId });
                Number = result;
            }
            else
            {
                throw new InvalidOperationException("Can't set the WorkItem.Number more than once.");
            }
        }

        public void TrackChanges(IDbConnection connection, int version, IEnumerable<PropertyChange> changes, IUser user)
        {
            // do nothing, we're using only async crud methods
        }

        public async Task TrackChangesAsync(IDbConnection connection, int version, IEnumerable<PropertyChange> changes, IUser user)
        {
            if (changes.Include(nameof(CloseReasonId)))
            {
                string closeReason = string.Empty;
                if (CloseReasonId.HasValue)
                {
                    closeReason = await connection.QuerySingleAsync<string>("SELECT [Name] FROM [app].[CloseReason] WHERE [Id]=@id", new { id = CloseReasonId });
                }
                string text = (CloseReasonId.HasValue) ? $"Work item {Number} was closed {closeReason}" : $"Work item {Number} was re-opened";
                await EventLog.WriteAsync(connection, new EventLog()
                {
                    WorkItemId = Id,
                    OrganizationId = OrganizationId,
                    TeamId = TeamId,
                    ApplicationId = ApplicationId,
                    EventId = (CloseReasonId.HasValue) ? SystemEvent.WorkItemClosed : SystemEvent.WorkItemOpened,
                    IconClass = (CloseReasonId.HasValue) ? IconClosed : "fas fa-play",
                    IconColor = (CloseReasonId.HasValue) ? "green" : "orange",
                    HtmlBody = text,
                    TextBody = text
                }, user);
            }

            if (changes.Include(nameof(MilestoneId), out PropertyChange milestoneChange))
            {
                await EventLog.WriteAsync(connection, new EventLog()
                {
                    WorkItemId = Id,
                    OrganizationId = OrganizationId,
                    TeamId = TeamId,
                    ApplicationId = ApplicationId,
                    EventId = SystemEvent.MilestoneChanged,
                    IconClass = "fas fa-flag-checkered",
                    HtmlBody = $"Milestone changed from {milestoneChange.OldValue ?? "<i>null</i>"} to {milestoneChange.NewValue ?? "<i>null</i>"}",
                    TextBody = $"Milestone changed from {milestoneChange.OldValue ?? "<null>"} to {milestoneChange.NewValue ?? "<null>"}"
                }, user);

                await ClosePlaceholderItemsAsync(connection, OrganizationId, MilestoneId, user);
            }

            if (changes.Include(nameof(ProjectId)))
            {
                // todo: log project change event
            }

            IEnumerable<PropertyChange> modifiedColumns;
            if (changes.Include(new string[] { nameof(Title) }, out modifiedColumns))
            {
                foreach (var col in modifiedColumns)
                {
                    await EventLog.WriteAsync(connection, new EventLog()
                    {
                        WorkItemId = Id,
                        OrganizationId = OrganizationId,
                        TeamId = TeamId,
                        ApplicationId = ApplicationId,
                        EventId = SystemEvent.WorkItemFieldChanged,
                        IconClass = "far fa-pencil",
                        HtmlBody = $"{col.PropertyName} changed from <em>{col.OldValue}</em> to <em>{col.NewValue}</em>",
                        TextBody = $"{col.PropertyName} changed from {col.OldValue} to {col.NewValue}"
                    }, user);
                }
            }
        }

        /// <summary>
        /// If there's a placeholder item and any other work item in the milestone, then the placeholder item is closed.
        /// </summary>
        private async Task ClosePlaceholderItemsAsync(IDbConnection connection, int orgId, int? milestoneId, IUser user)
        {
            if (!milestoneId.HasValue) return;

            int? placeholderLabelId = await connection.QuerySingleOrDefaultAsync<int?>("SELECT [Id] FROM [dbo].[Label] WHERE [OrganizationId]=@orgId AND [Name]=@name", new { orgId, name = Label.PlaceholderLabel });
            if (!placeholderLabelId.HasValue) return;

            var parameters = new { orgId, msId = milestoneId, labelId = placeholderLabelId };

            var placeholderItemIds = await connection.QueryAsync<int>(
                "SELECT [wi].[Id] FROM [dbo].[WorkItem] [wi] WHERE [OrganizationId]=@orgId AND [MilestoneId]=@msId AND EXISTS(SELECT 1 FROM [dbo].[WorkItemLabel] WHERE [WorkItemId]=[wi].[Id] AND [LabelId]=@labelId)",
                parameters);

            var realItemIds = await connection.QueryAsync<int>(
                "SELECT [wi].[Id] FROM [dbo].[WorkItem] [wi] WHERE [OrganizationId]=@orgId AND [MilestoneId]=@msId AND NOT EXISTS(SELECT 1 FROM [dbo].[WorkItemLabel] WHERE [WorkItemId]=[wi].[Id] AND [LabelId]=@labelId)",
                parameters);

            if (placeholderItemIds.Any() && realItemIds.Any())
            {
                foreach (int id in placeholderItemIds)
                {
                    var comment = new Comment()
                    {
                        OrganizationId = orgId,
                        ObjectId = id,
                        ObjectType = ObjectType.WorkItem,
                        TextBody = "Placeholder item auto-closed",
                        HtmlBody = "<p>Placeholder item auto-closed</p>"
                    };
                    await connection.SaveAsync(comment, user);
                    await connection.ExecuteAsync("UPDATE [wi] SET [CloseReasonId]=8 FROM [dbo].[WorkItem] [wi] WHERE [Id]=@id", new { id });
                }
            }
        }

        /// <summary>
        /// For event logging purposes, we need to get the orgId associated with an IFeedItem (unless it's already part of the item itself, e.g. WorkItem).
        /// This static method provides a standard way to get this when the workItemId is already known
        /// </summary>
        internal static async Task<OrgAndApp> GetOrgAndAppIdAsync(IDbConnection connection, int workItemId)
        {
            return await connection.QuerySingleAsync<OrgAndApp>("SELECT [OrganizationId], [ApplicationId] FROM [dbo].[WorkItem] WHERE [Id]=@workItemId", new { workItemId });
        }

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return await Task.FromResult(OrganizationId);
        }

        public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider)
        {
            WorkItemTicket = connection.FindWhere<WorkItemTicket>(new { this.OrganizationId, WorkItemNumber = Number });
            Organization = connection.Find<Organization>(OrganizationId);
        }

        public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider)
        {
            WorkItemTicket = await connection.FindWhereAsync<WorkItemTicket>(new { this.OrganizationId, WorkItemNumber = Number });
            Organization = await connection.FindAsync<Organization>(OrganizationId);
        }
    }
}