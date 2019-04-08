using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class Applications : Query<Application>
	{
		public Applications() : base(
			@"SELECT [app].* FROM [dbo].[Application] [app]
			WHERE [OrganizationId]=@orgId AND [IsActive]=@isActive {andWhere}
			ORDER BY [Name]")
		{
		}

		public int OrgId { get; set; }
		public bool IsActive { get; set; }

		[Where("[Id]=@id")]
		public int? Id { get; set; }
	}
}