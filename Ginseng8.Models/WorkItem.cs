using Dapper;
using Ginseng.Models.Conventions;
using Ginseng.Models.Extensions;
using Ginseng.Models.Interfaces;
using Ginseng.Models.Internal;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.Base.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	/// <summary>
	/// User story, development task, bug, or feature request
	/// </summary>
	[TrackChanges(IgnoreProperties = "ModifiedBy;DateModified")]
	public class WorkItem : BaseTable, IBody, ITrackedRecord
	{
		public const string IconClass = "far fa-plus-hexagon";

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

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		/// <summary>
		/// Indicates that a comment was added with IsImpediment = true
		/// </summary>
		[DefaultExpression("0")]
		public bool HasImpediment { get; set; }

		[References(typeof(Application))]
		public int ApplicationId { get; set; }

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

		public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action, IUser user)
		{
			// todo: label parsing from title
			/*
			if (action == SaveAction.Insert)
			{
				int[] labelIds = await ParseLabelsAsync(connection, Title);
			}*/

			if (action == SaveAction.Insert)
			{
				await EventLog.WriteAsync(connection, new EventLog()
				{
					WorkItemId = Id,
					OrganizationId = OrganizationId,
					ApplicationId = ApplicationId,
					EventId = SystemEvent.WorkItemCreated,
					HtmlBody = Title,
					TextBody = Title,
					IconClass = IconClass
				}, user);
			}
		}

		private Task<int[]> ParseLabelsAsync(IDbConnection connection, string title)
		{
			throw new NotImplementedException();
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
				string text = (CloseReasonId.HasValue) ? $"Work item {Number} was closed" : $"Work item {Number} was re-opened";
				await EventLog.WriteAsync(connection, new EventLog()
				{
					WorkItemId = Id,
					OrganizationId = OrganizationId,
					ApplicationId = ApplicationId,
					EventId = (CloseReasonId.HasValue) ? SystemEvent.WorkItemClosed : SystemEvent.WorkItemOpened,
					IconClass = (CloseReasonId.HasValue) ? "fas fa-clipboard-check" : "fas fa-play",
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
					ApplicationId = ApplicationId,
					EventId = SystemEvent.MilestoneChanged,
					IconClass = "fas fa-flag-checkered",
					HtmlBody = $"Milestone changed from {milestoneChange.OldValue} to {milestoneChange.NewValue}",
					TextBody = $"Milestone changed from {milestoneChange.OldValue} to {milestoneChange.NewValue}"
				}, user);
			}

			if (changes.Include(nameof(ProjectId)))
			{
				// todo: log project change event
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
	}
}