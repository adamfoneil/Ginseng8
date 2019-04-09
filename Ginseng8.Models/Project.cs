using Dapper;
using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	/// <summary>
	/// A collection of work items centered around a single goal, feature, or some other unifying idea
	/// </summary>
	public class Project : BaseTable, IBody, IFindRelated<int>
	{
		[References(typeof(Application))]
		[PrimaryKey]
		public int ApplicationId { get; set; }

		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		[MaxLength(255)]
		public string Description { get; set; }

		/// <summary>
		/// Overall priority of project (lower number = higher priority)
		/// </summary>
		public int? Priority { get; set; }

		/// <summary>
		/// Source code location
		/// </summary>
		[MaxLength(255)]
		public string BranchUrl { get; set; }

		[References(typeof(DataModel))]
		public int? DataModelId { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		public bool IsActive { get; set; } = true;

		public Application Application { get; set; }

		public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action, IUser user)
		{
			if (action == SaveAction.Update) await SyncWorkItemsToProjectAsync(connection);
		}

		public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider)
		{
			Application = commandProvider.Find<Application>(connection, ApplicationId);
		}

		public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider)
		{
			Application = await commandProvider.FindAsync<Application>(connection, ApplicationId);
		}

		public async Task SyncWorkItemsToProjectAsync(IDbConnection connection)
		{
			await connection.ExecuteAsync(
				@"UPDATE [dbo].[WorkItem] SET [ApplicationId]=@appId WHERE [ProjectId]=@projectId",
				new { appId = ApplicationId, projectId = Id });
		}
	}
}