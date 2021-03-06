﻿using Dapper;
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
using System.Security.Claims;
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
        public const string IconAssigned = "far fa-clipboard-list";

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
        public Team Team { get; set; }
        public Organization Organization { get; set; }

        /// <summary>
        /// Default label to create with item
        /// </summary>
        [NotMapped]
        public int LabelId { get; set; }

        [NotMapped]
        public int? AssignToUserId { get; set; }

        /// <summary>
        /// this is int instead of bool so it works with InsertItemView, whose initializer accepts ints only
        /// </summary>
        [NotMapped]
        public int? IsPinned { get; set; }

        public override async Task BeforeSaveAsync(IDbConnection connection, SaveAction action, IUser user)
        {
            await base.BeforeSaveAsync(connection, action, user);

            // resolve indirect milestone
        }

        public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action, IUser user)
        {
            if (action == SaveAction.Insert)
            {
                if (LabelId != 0)
                {
                    var wil = new WorkItemLabel() { WorkItemId = Id, LabelId = LabelId };
                    await connection.SaveAsync(wil, user);
                }

                if ((IsPinned ?? 0) == 1)
                {
                    PinnedWorkItem pin = new PinnedWorkItem() { WorkItemId = Id, UserId = DeveloperUserId ?? BusinessUserId ?? 0 };
                    if (pin.UserId != 0)
                    {
                        await connection.SaveAsync(pin, user);
                    }
                }

                var workItemLabels = await ParseLabelsAsync(connection);

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

                foreach (var wil in workItemLabels) 
                {
                    if (wil.LabelId != LabelId) // don't regen the same event for a label already recorded
                    {
                        await wil.AfterSaveAsync(connection, SaveAction.Insert, user);
                    }                    
                }
            }

            await ClosePlaceholderItemsAsync(connection, OrganizationId, MilestoneId, ProjectId, user);
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
					WHERE [TeamId]=@teamId AND
					([Name] LIKE '%' + @projectName + '%' OR [Nickname]=@projectName)", new { teamId = TeamId, projectName = projectToken });

                if (projectId != 0)
                {
                    var prj = await connection.FindAsync<Project>(projectId);
                    ProjectId = projectId;
                    if (!ApplicationId.HasValue) ApplicationId = prj.ApplicationId;
                    await connection.UpdateAsync(this, null, r => r.ProjectId, r => r.ApplicationId);
                }
            }
        }

        private async Task<IEnumerable<WorkItemLabel>> ParseLabelsAsync(IDbConnection connection)
        {
            var labelMatches = Regex.Matches(Title, @"#\w*");
            var labelNames = labelMatches.Cast<Match>().Select(m => m.Value.Substring(1)).Where(s => s.Length >= 2).ToArray();

            if (labelNames.Any())
            {
                string nameCriteria = string.Join(" OR ", labelNames.Select(label => $"[Name] LIKE '%{label}%'"));
                await connection.ExecuteAsync(
                    @"INSERT INTO [dbo].[WorkItemLabel] (
						[WorkItemId], [LabelId], [CreatedBy], [DateCreated]
					) SELECT
						@workItemId, [Id], @userName, @dateCreated
					FROM
						[dbo].[Label]
					WHERE						
						[OrganizationId]=@orgId AND [IsActive]=1 AND (" + nameCriteria + ")",
                    new { workItemId = Id, orgId = OrganizationId, userName = CreatedBy, dateCreated = DateCreated });

                string newTitle = Title;
                foreach (var label in labelNames) newTitle = newTitle.Replace("#" + label, string.Empty);
                await connection.ExecuteAsync(
                    "UPDATE [wi] SET [Title]=@title FROM [dbo].[WorkItem] [wi] WHERE [Id]=@id AND [OrganizationId]=@orgId",
                    new { id = Id, orgId = OrganizationId, title = newTitle.Trim() });                
            }

            return await connection.QueryAsync<WorkItemLabel>("SELECT * FROM [dbo].[WorkItemLabel] WHERE [WorkItemId]=@workItemId", new { workItemId = Id });
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

                await ClosePlaceholderItemsAsync(connection, OrganizationId, MilestoneId, ProjectId, user);
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
        private async Task ClosePlaceholderItemsAsync(IDbConnection connection, int orgId, int? milestoneId, int? projectId, IUser user)
        {
            if (!milestoneId.HasValue) return;

            int? placeholderLabelId = await connection.QuerySingleOrDefaultAsync<int?>(
                "SELECT [Id] FROM [dbo].[Label] WHERE [OrganizationId]=@orgId AND [Name]=@name", new { orgId, name = Label.PlaceholderLabel });
            if (!placeholderLabelId.HasValue) return;

            var parameters = new { orgId, msId = milestoneId, labelId = placeholderLabelId, projectId };

            var placeholderItemIds = await connection.QueryAsync<int>(
                "SELECT [wi].[Id] FROM [dbo].[WorkItem] [wi] WHERE [OrganizationId]=@orgId AND [ProjectId]=@projectId AND [MilestoneId]=@msId AND EXISTS(SELECT 1 FROM [dbo].[WorkItemLabel] WHERE [WorkItemId]=[wi].[Id] AND [LabelId]=@labelId)",
                parameters);

            var realItemIds = await connection.QueryAsync<int>(
                "SELECT [wi].[Id] FROM [dbo].[WorkItem] [wi] WHERE [OrganizationId]=@orgId AND [ProjectId]=@projectId AND [MilestoneId]=@msId AND NOT EXISTS(SELECT 1 FROM [dbo].[WorkItemLabel] WHERE [WorkItemId]=[wi].[Id] AND [LabelId]=@labelId)",
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
        internal static async Task<OrgTeamApp> GetOrgAndAppIdAsync(IDbConnection connection, int workItemId)
        {
            return await connection.QuerySingleAsync<OrgTeamApp>("SELECT [OrganizationId], [TeamId], [ApplicationId] FROM [dbo].[WorkItem] WHERE [Id]=@workItemId", new { workItemId });
        }

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return await Task.FromResult(OrganizationId);
        }

        public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider, IUser user = null, IEnumerable<Claim> claims = null)
        {
            Team = connection.Find<Team>(TeamId);
            WorkItemTicket = connection.FindWhere<WorkItemTicket>(new { this.OrganizationId, WorkItemNumber = Number });
            Organization = connection.Find<Organization>(OrganizationId);
        }

        public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider, IUser user = null, IEnumerable<Claim> claims = null)
        {
            Team = await connection.FindAsync<Team>(TeamId);
            WorkItemTicket = await connection.FindWhereAsync<WorkItemTicket>(new { this.OrganizationId, WorkItemNumber = Number });
            Organization = await connection.FindAsync<Organization>(OrganizationId);
        }
    }
}