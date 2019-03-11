using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class Projects : Query<Project>
	{
		public Projects() : base("SELECT * FROM [dbo].[Project] WHERE [OrganizationId]=@orgId AND [IsActive]=@isActive {andWhere} ORDER BY [Priority], [Name]")
		{
		}

		public int OrgId { get; set; }
		public bool IsActive { get; set; }

		[Where("[Id] IN @includeIds")]
		public int[] IncludeIds { get; set; }
	}
}