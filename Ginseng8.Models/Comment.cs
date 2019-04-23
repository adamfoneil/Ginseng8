using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Ginseng.Models.Queries;
using Html2Markdown;
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
			// for now, can do mentions only on work item comments because EventLog.WorkItemId is required
			if (comment.ObjectType != ObjectType.WorkItem) return;

			var names = Regex.Matches(comment.TextBody, "@([a-zA-Z][a-zA-Z0-9_]*)").OfType<Match>();
			if (!names.Any()) return;

			foreach (var name in names)
			{
				var users = await new OrgUserByName() { OrgId = comment.OrganizationId, Name = name.Value.Substring(1) }.ExecuteAsync(connection);
				if (users.Any())
				{
					var orgUser = users.First();
					await ReplaceMentionNameAsync(connection, comment, name.Value, orgUser);
					var eventLog = CreateEventLogFromCommentAsync(connection, comment);
					await Notification.CreateFromMentionAsync(connection, comment, orgUser);
				}
			}
		}

		private async Task<EventLog> CreateEventLogFromCommentAsync(IDbConnection connection, Comment comment)
		{
			var workItem = await connection.FindAsync<WorkItem>(comment.ObjectId);

			return new EventLog()
			{
				OrganizationId = comment.OrganizationId,
				ApplicationId = workItem.ApplicationId,
				WorkItemId = comment.ObjectId,
				EventId = SystemEvent.UserMentioned,
				IconClass = "fas fa-at",
				IconColor = "auto",
				HtmlBody = comment.HtmlBody, // should be mention names, not comment body
				TextBody = comment.TextBody,
				SourceId = comment.Id,
				SourceTable = nameof(Comment)
			};
		}

		private async Task ReplaceMentionNameAsync(IDbConnection connection, Comment comment, string mentionName, OrganizationUser orgUser)
		{
			string result = comment.HtmlBody;
			result = result.Replace(mentionName, $"<a href=\"mailto:{orgUser.Email}\">{orgUser.DisplayName ?? orgUser.UserName}</a>");
			comment.HtmlBody = result;
			comment.TextBody = new Converter().Convert(comment.HtmlBody);
			await connection.SaveAsync(comment);
		}		
	}
}