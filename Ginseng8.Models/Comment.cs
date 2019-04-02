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
	/// <summary>
	/// Info added to a work item
	/// </summary>
	public class Comment : BaseTable, IBody
	{
		public const string CommentIcon = "far fa-comment";
		public const string ImpedimentIcon = "fas fa-comment-times";
		public const string ResolvedIcon = "fas fa-comment-check";

		[References(typeof(WorkItem))]
		public int WorkItemId { get; set; }		

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

		/// <summary>
		/// For forms where work item number is used instead of Id
		/// </summary>
		[NotMapped]
		public int Number { get; set; }

		public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action, IUser user)
		{
			await base.AfterSaveAsync(connection, action, user);

			if (action == SaveAction.Insert)
			{
				if (IsImpediment.HasValue)
				{
					var workItem = await connection.FindAsync<WorkItem>(WorkItemId);
					workItem.HasImpediment = IsImpediment.Value;
					await connection.UpdateAsync(workItem, user, r => r.HasImpediment);
				}

				await EventLog.WriteAsync(connection, new EventLog(WorkItemId, user)
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