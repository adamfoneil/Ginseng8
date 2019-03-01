using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class Applications : Query<Application>
	{
		public Applications() : base(
			@"SELECT * FROM [dbo].[Application] 
			WHERE [OrganizationId]=@orgId AND [IsActive]=@isActive 
			ORDER BY [Name]")
		{
		}

		public int OrgId { get; set; }
		public bool IsActive { get; set; }
	}
}