using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	/// <summary>
	/// Test instructons or feedback, request for code review, or another other change in activity on a work item
	/// </summary>
	public class HandOff : BaseTable, IBody
	{
		public const string ForwardHandOff = "fas fa-chevron-circle-right";
		public const string BackwardHandOff = "far fa-chevron-circle-left";

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

		public string IconClass => GetIconClass(IsForward);

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

		public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action, IUser user)
		{
			if (action == SaveAction.Insert)
			{
				var workItem = await connection.FindAsync<WorkItem>(WorkItemId);
				workItem.ActivityId = ToActivityId;
				workItem.LastHandOffId = Id;										
				await connection.SaveAsync(workItem, user);

				var toActivity = await connection.FindAsync<Activity>(ToActivityId);
				var fromActivity = await connection.FindAsync<Activity>(FromActivityId);
				string displayUser = await OrganizationUser.GetUserDisplayNameAsync(connection, workItem.OrganizationId, FromUserId, user);
				string text = $"{displayUser} handed off work item {workItem.Number} from {fromActivity.Name} to {toActivity.Name}";

				int eventLogId = await EventLog.WriteAsync(connection, new EventLog(WorkItemId, user)
				{
					EventId = (IsForward) ? SystemEvent.HandOffForward : SystemEvent.HandOffBackward,
					IconClass = GetIconClass(IsForward),
					IconColor = GetColor(IsForward),
					HtmlBody = text,
					TextBody = text,
					SourceId = Id,
					SourceTable = nameof(HandOff)
				});
				
				await Notification.CreateFromActivitySubscriptions(connection, eventLogId);				
			}
		}
	}
}