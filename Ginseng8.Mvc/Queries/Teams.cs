using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class Teams : Query<Team>
    {
        public Teams() : base("SELECT * FROM [dbo].[Team] WHERE [OrganizationId]=@orgId {andWhere} ORDER BY [Name]")
        {
        }

        public int OrgId { get; set; }

        [Where("[IsActive]=@isActive")]
        public bool? IsActive { get; set; } = true;
    }
}