using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;
using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;

namespace Ginseng.Models
{
	/// <summary>
	/// Test instructons or feedback, request for code review, or another other change in activity on a work item
	/// </summary>
	public class HandOff : BaseTable, IFeedItem, IBody
	{
		public const string ForwardHandOff = "fas fa-chevron-circle-right";
		public const string BackwardHandOff = "far fa-chevron-circle-left";

		/// <summary>
		/// workaround for Postulate limitation 
		/// https://github.com/adamosoftware/Postulate/issues/15
		/// </summary>
		private IUser _user;

		public static string GetIconClass(bool isForward)
		{
			return (isForward) ? ForwardHandOff : BackwardHandOff;
		}

		public static string GetColor(bool isForward)
		{
			return (isForward) ? "green" : "orange";
		}

		[References(typeof(WorkItem))]
		public int WorkItemId { get; set; }

		public string IconClass => (IsForward) ? ForwardHandOff : BackwardHandOff;

		[References(typeof(UserProfile))]
		public int FromUserId { get; set; }

		[References(typeof(Activity))]
		public int FromActivityId { get; set; }

		[References(typeof(Activity))]
		public int ToActivityId { get; set; }

		/// <summary>
		/// Indicates whether the from and to activities move forward through workflow
		/// </summary>
		public bool IsForward { get; set; }

		public string HtmlBody { get; set; }

		public string TextBody { get; set; }

		[NotMapped]
		public int OrganizationId { get; set; }

		[NotMapped]
		public int ApplicationId { get; set; }

		[NotMapped]
		public SystemEvent EventId { get; set; } = SystemEvent.HandOff;

		public override void BeforeSave(IDbConnection connection, SaveAction action, IUser user)
		{
			base.BeforeSave(connection, action, user);
			_user = user;
		}

		public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action)
		{
			if (action == SaveAction.Insert)
			{
				var workItem = await connection.FindAsync<WorkItem>(WorkItemId);
				workItem.ActivityId = ToActivityId;

				// clear the user id from the column implied by the activity responsibility
				// in order to make room for someone to take the work item
				var activity = await connection.FindAsync<Activity>(ToActivityId);
				Responsibility.ClearWorkItemUserActions[activity.ResponsibilityId].Invoke(workItem);

				await connection.SaveAsync(workItem, _user);

				await EventLog.LogAsync(connection, this);
			}			
		}

		public async Task SetOrgAndAppIdAsync(IDbConnection connection)
		{
			(int, int) result = await WorkItem.GetOrgAndAppIdAsync(connection, WorkItemId);
			OrganizationId = result.Item1;
			ApplicationId = result.Item2;
		}
	}
}