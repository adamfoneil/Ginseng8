using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Ginseng.Models.Queries;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	public enum ObjectType
	{
		WorkItem = 1,
		Project = 2,
		DataModel = 3,
		ModelClass = 4,
		Article = 5
	}

	/// <summary>
	/// Info added to a work item
	/// </summary>
	public class Comment : BaseTable, IBody
	{
		public const string CommentIcon = "far fa-comment";
		public const string ImpedimentIcon = "fas fa-comment-times";
		public const string ResolvedIcon = "fas fa-comment-check";
		
		[References(typeof(Organization))]		
		public int OrganizationId { get; set; }

		public ObjectType ObjectType { get; set; } = ObjectType.WorkItem;

		public int ObjectId { get; set; }

		public bool? IsImpediment { get; set; }

		public string IconClass => (!IsImpediment.HasValue) ?
			CommentIcon :
				(IsImpediment.Value) ?
					ImpedimentIcon :
					ResolvedIcon;

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		[NotMapped]
		public string DisplayName { get; set; }

		public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action, IUser user)
		{
			await base.AfterSaveAsync(connection, action, user);

			if (action == SaveAction.Insert)
			{
				if (IsImpediment.HasValue && ObjectType == ObjectType.WorkItem)
				{
					var workItem = await connection.FindAsync<WorkItem>(ObjectId);
					workItem.HasImpediment = IsImpediment.Value;
					await connection.UpdateAsync(workItem, user, r => r.HasImpediment);
				}

				if (ObjectType == ObjectType.WorkItem)
				{
					await EventLog.WriteAsync(connection, new EventLog(ObjectId, user)
					{
						EventId = (IsImpediment ?? false) ? SystemEvent.ImpedimentAdded : SystemEvent.CommentAdded,
						IconClass = IconClass,
						IconColor = (IsImpediment ?? false) ? "darkred" : "auto",
						HtmlBody = HtmlBody,
						TextBody = TextBody
					});
				}

				await PendingWorkLog.FromCommentAsync(connection, this, user as UserProfile);
				await ParseMentionsAsync(connection, this, user as UserProfile);
			}
		}

		/// <summary>
		/// Queues notifications to people from comment text based on @ symbols 
		/// </summary>		
		private async Task ParseMentionsAsync(IDbConnection connection, Comment comment, UserProfile userProfile)
		{
			var names = Regex.Matches(comment.TextBody, "@([a-zA-Z][a-zA-Z0-9_]*)").OfType<Match>();
			if (!names.Any()) return;

			foreach (var name in names)
			{
				var users = await new OrgUserByName() { OrgId = comment.OrganizationId, Name = name.Value.Substring(1) }.ExecuteAsync(connection);
				if (users.Any())
				{
					var eventLog = await EventLogFromCommentAsync(connection, comment);					
					int eventLogId = await EventLog.WriteAsync(connection, eventLog, userProfile);
					// there might be multiple names found from the name entered, but we're just picking one to prevent accidental spamming
					await Notification.CreateFromMentionAsync(connection, eventLogId, comment, users.First(), name.Value);
				}
			}
		}

		/// <summary>
		/// Sets the appId, sourceId, and workItemId based on the context of the comment object type
		/// </summary>
		private async Task<EventLog> EventLogFromCommentAsync(IDbConnection connection, Comment comment)
		{
			var result = new EventLog()
			{
				OrganizationId = comment.OrganizationId,
				EventId = SystemEvent.UserMentioned,
				IconClass = "fas fa-at",
				IconColor = "blue",
				HtmlBody = comment.HtmlBody,
				TextBody = comment.TextBody
			};

			switch (comment.ObjectType)
			{
				case ObjectType.WorkItem:
					var workItem = await connection.FindAsync<WorkItem>(comment.ObjectId);
					result.WorkItemId = comment.ObjectId;
					result.ApplicationId = workItem.ApplicationId;
					break;

				case ObjectType.Project:
					var prj = await connection.FindAsync<Project>(comment.ObjectId);
					result.ApplicationId = prj.ApplicationId;
					result.SourceId = comment.ObjectId;
					result.SourceTable = nameof(Project);
					break;

				default:
					result.SourceId = comment.ObjectId;
					result.SourceTable = "unknown"; // will have to figure these out eventually
					break;
			}

			return result;
		}
	}
}