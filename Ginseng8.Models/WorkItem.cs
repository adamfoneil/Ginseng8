using Dapper;
using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	/// <summary>
	/// User story, development task, bug, or feature request
	/// </summary>
	public class WorkItem : BaseTable, IBody
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
		public int? OwnerUserId { get; set; }

		/// <summary>
		/// Overall priority of work item (lower number = higher priority)
		/// </summary>
		public int? Priority { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		[References(typeof(Application))]
		public int ApplicationId { get; set; }

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

		public async Task SetNumber(IDbConnection connection)
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
	}
}