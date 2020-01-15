using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;

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

        [Where("[AllowNewItems]=@allowNewItems")]
        public bool? AllowNewItems { get; set; }

		[Where("[Id]=@id")]
		public int? Id { get; set; }

        [Where("[TeamId]=@teamId")]
        public int? TeamId { get; set; }
	}
}