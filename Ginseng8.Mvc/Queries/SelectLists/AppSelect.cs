using Ginseng.Mvc.Classes;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class AppSelect : SelectListQuery
	{
		public AppSelect() : base(
			@"SELECT [Id] AS [Value], [Name] AS [Text]
			FROM [dbo].[Application] [app]
			WHERE [OrganizationId]=@orgId AND [IsActive]=1 {andWhere}
			ORDER BY [Name]")
		{
		}

		public int OrgId { get; set; }

        [Where("[app].[TeamId]=@teamId")]
        public int? TeamId { get; set; }
	}
}