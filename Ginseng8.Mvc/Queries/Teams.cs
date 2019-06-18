using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
    public class Teams : Query<Team>
    {
        public Teams() : base(
            @"SELECT 
                [t].*,
                (SELECT COUNT(1) FROM [dbo].[Project] WHERE [TeamId]=[t].[Id] AND [IsActive]=1) AS [ActiveProjects],
                (SELECT COUNT(1) FROM [dbo].[Project] WHERE [TeamId]=[t].[Id] AND [IsActive]=0) AS [InactiveProjects],
            FROM [dbo].[Team] [t]
            WHERE [OrganizationId]=@orgId {andWhere} 
            ORDER BY [Name]")
        {
        }

        public int OrgId { get; set; }

        [Where("[IsActive]=@isActive")]
        public bool? IsActive { get; set; } = true;

        [Where("[Id]=@id")]
        public int? Id { get; set; }
    }
}