using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
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
			}
		}
	}
}