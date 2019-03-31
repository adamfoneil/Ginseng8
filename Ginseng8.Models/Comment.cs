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
	public class Comment : BaseTable, IFeedItem, IBody
	{
		private IUser _user;

		[References(typeof(WorkItem))]
		public int WorkItemId { get; set; }

		public string IconClass => (!IsImpediment.HasValue) ?
			"far fa-comment" :
				(IsImpediment.Value) ?
					"far fa-comment-times" :
					"far fa-comment-check";

		public bool? IsImpediment { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		[NotMapped]
		public string DisplayName { get; set; }

		/// <summary>
		/// For forms where work item number is used instead of Id
		/// </summary>
		[NotMapped]
		public int Number { get; set; }

		[NotMapped]
		public int OrganizationId { get; set; }

		[NotMapped]
		public int ApplicationId { get; set; }

		[NotMapped]
		public SystemEvent EventId { get; set; }

		public override void BeforeSave(IDbConnection connection, SaveAction action, IUser user)
		{
			base.BeforeSave(connection, action, user);
			_user = user;
		}

		public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action)
		{
			await base.AfterSaveAsync(connection, action);

			if (action == SaveAction.Insert)
			{
				if (IsImpediment.HasValue)
				{
					var workItem = await connection.FindAsync<WorkItem>(WorkItemId);
					workItem.HasImpediment = IsImpediment.Value;
					await connection.UpdateAsync(workItem, _user, r => r.HasImpediment);
				}

				EventId = (IsImpediment ?? false) ? SystemEvent.ImpedimentAdded : SystemEvent.CommentAdded;
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