using Dapper;
using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    /// <summary>
    /// A project's parent container varies depending on whether the Team uses apps or not.
    /// This enum provides a way to navigate the Dashboard/Org
    /// </summary>
    public enum ProjectParentType
    {
        Team,
        Application
    }

	/// <summary>
	/// A collection of work items centered around a single goal, feature, or some other unifying idea or theme
	/// </summary>
	public class Project : BaseTable, IBody, IFindRelated<int>, IOrgSpecific
	{
		[References(typeof(Team))]
		[PrimaryKey]
		public int TeamId { get; set; }

		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

        [References(typeof(Application))]
        public int? ApplicationId { get; set; }

		[MaxLength(255)]
		public string Description { get; set; }

		/// <summary>
		/// Overall priority of project (lower number = higher priority)
		/// </summary>
		public int? Priority { get; set; }

        /// <summary>
        /// Shown on milestone dashboard
        /// </summary>
        [MaxLength(5)]
        public string Nickname { get; set; }

		/// <summary>
		/// Source code location
		/// </summary>
		[MaxLength(255)]
		public string BranchUrl { get; set; }

		[References(typeof(DataModel))]
		public int? DataModelId { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

        public long? FreshdeskCompanyId { get; set; }

		public bool IsActive { get; set; } = true;

        public Team Team { get; set; }
		public Application Application { get; set; }

        public string ParentName
        {
            get { return Application?.Name ?? Team?.Name; }
        }

        public int ParentId
        {
            get { return (Team.UseApplications) ? (ApplicationId ?? 0) : TeamId; }
        }

        public ProjectParentType ParentType
        {
            get { return (Team?.UseApplications ?? false) ? ProjectParentType.Application : ProjectParentType.Team; }
        }

		public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action, IUser user)
		{
			if (action == SaveAction.Update) await SyncWorkItemsToProjectAsync(connection);
		}

		public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider)
		{
            Team = commandProvider.Find<Team>(connection, TeamId);
			if (ApplicationId.HasValue) Application = commandProvider.Find<Application>(connection, ApplicationId.Value);
		}

		public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider)
		{
            Team = await commandProvider.FindAsync<Team>(connection, TeamId);
			if (ApplicationId.HasValue) Application = await commandProvider.FindAsync<Application>(connection, ApplicationId.Value);
		}

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            var team = await connection.FindAsync<Team>(TeamId);
            return team.OrganizationId;
        }

        public async Task SyncWorkItemsToProjectAsync(IDbConnection connection)
		{
			await connection.ExecuteAsync(
				@"UPDATE [dbo].[WorkItem] SET [ApplicationId]=@appId WHERE [ProjectId]=@projectId",
				new { appId = ApplicationId, projectId = Id });
		}
	}
}