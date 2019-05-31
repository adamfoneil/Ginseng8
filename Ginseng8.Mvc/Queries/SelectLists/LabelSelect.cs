using Ginseng.Mvc.Classes;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class LabelSelect : SelectListQuery
	{
		public LabelSelect() : base(
			@"SELECT [Id] AS [Value], [Name] AS [Text]
			FROM [dbo].[Label]
			WHERE [OrganizationId]=@orgId AND [IsActive]=1 {andWhere}
			ORDER BY [Name]")
		{
		}

		public int OrgId { get; set; }

        [Where("[AllowNewItems]=@allowNewItems")]
        public bool? AllowNewItems { get; set; }
	}
}