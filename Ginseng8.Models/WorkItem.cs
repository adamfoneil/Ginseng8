using Dapper;
using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
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

		public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action)
		{
			await Task.CompletedTask;
			// todo: label parsing from title
			/*
			if (action == SaveAction.Insert)
			{
				int[] labelIds = await ParseLabelsAsync(connection, Title);
			}*/
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// For event logging purposes, we need to get the orgId associated with an IFeedItem (unless it's already part of the item itself, e.g. WorkItem).
		/// This static method provides a standard way to get this in IFeedItem implementations when a workItemId is already known
		/// </summary>
		public static async Task<int> GetOrgIdAsync(IDbConnection connection, int workItemId)
		{
			return await connection.QuerySingleAsync<int>("SELECT [OrganizationId] FROM [dbo].[WorkItem] WHERE [Id]=@workItemId", new { workItemId });
		}
	}
}