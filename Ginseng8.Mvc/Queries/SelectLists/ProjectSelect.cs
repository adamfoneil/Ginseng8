using Ginseng.Mvc.Classes;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries.SelectLists
{
    public class ProjectSelectResult
    {
        public int Value { get; set; }
        public string Text { get; set; }
        public long? FreshdeskCompanyId { get; set; }
    }

    public class ProjectSelectEx : Query<ProjectSelectResult>
    {
        public const string BaseSql = 
            @"SELECT [p].[Id] AS [Value], COALESCE([p].[Nickname], [p].[Name]) AS [Text], [p].[FreshdeskCompanyId]
			FROM [dbo].[Project] [p]		
            INNER JOIN [dbo].[Team] [t] ON [p].[TeamId]=[t].[Id]
			WHERE [p].[IsActive]=1 {andWhere}
			ORDER BY [p].[Name]";

        public ProjectSelectEx() : base(BaseSql)
        {
        }

        [Where("[p].[TeamId]=@teamId")]
        public int? TeamId { get; set; }

        [Where("[t].[OrganizationId]=@orgId")]
        public int? OrgId { get; set; }

        [Where("[p].[ApplicationId]=@appId")]
        public int? AppId { get; set; }
    }

    public class ProjectSelect : SelectListQuery
	{
		public ProjectSelect() : base(ProjectSelectEx.BaseSql)
		{
		}

        [Where("[p].[TeamId]=@teamId")]
		public int? TeamId { get; set; }

        [Case(0, "[p].[ApplicationId] IS NULL")]
        [Where("[p].[ApplicationId]=@appId")]
        public int? AppId { get; set; }

        [Where("[p].[Id]<>@excludeId")]
        public int? ExcludeId { get; set; }
	}
}