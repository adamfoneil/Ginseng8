using Ginseng.Mvc.Classes;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class ActivitySelect : SelectListQuery
	{
		public ActivitySelect() : base(
			@"SELECT [a].[Id] AS [Value], [a].[Name] AS [Text]
			FROM [dbo].[Activity] [a]
			INNER JOIN [app].[Responsibility] [r] ON [a].[ResponsibilityId]=[r].[Id]
			WHERE [OrganizationId]=@orgId AND [IsActive]=1 {andWhere}
			ORDER BY [Order]")
		{
		}

		public int OrgId { get; set; }

		[Where("((@responsibilities & [r].[Flag]) = [r].[Flag])")]
		public int? Responsibilities { get; set; }
	}
}