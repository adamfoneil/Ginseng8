using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class Activities : Query<Activity>
	{
		public Activities() : base(
			@"SELECT [a].* 
			FROM [dbo].[Activity] [a]
			INNER JOIN [app].[Responsibility] [r] ON [a].[ResponsibilityId]=[r].[Id]
			WHERE [OrganizationId]=@orgId AND [IsActive]=@isActive {andWhere}
			ORDER BY [Order], [Name]")
		{
		}

		public int OrgId { get; set; }
		public bool IsActive { get; set; }

		[Where("((@responsibilities & [r].[Flag]) = [r].[Flag])")]
		public int? Responsibilities { get; set; }
	}
}