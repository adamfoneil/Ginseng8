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
            @"SELECT [p].[Id] AS [Value], [p].[Name] AS [Text], [p].[FreshdeskCompanyId]
			FROM [dbo].[Project] [p]		
            INNER JOIN [dbo].[Application] [app] ON [p].[ApplicationId]=[app].[Id]
			WHERE [p].[IsActive]=1 {andWhere}
			ORDER BY [p].[Name]";

        public ProjectSelectEx() : base(BaseSql)
        {
        }

        [Where("[p].[ApplicationId]=@appId")]
        public int? AppId { get; set; }

        [Where("[app].[OrganizationId]=@orgId")]
        public int? OrgId { get; set; }
    }

    public class ProjectSelect : SelectListQuery
	{
		public ProjectSelect() : base(ProjectSelectEx.BaseSql)
		{
		}

        [Where("[p].[ApplicationId]=@appId")]
		public int? AppId { get; set; }
	}
}