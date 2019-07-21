using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
    public class TeamSelect : SelectListQuery
    {
        public TeamSelect() : base(
            @"SELECT [Id] AS [Value], [Name] AS [Text] 
            FROM [dbo].[Team] 
            WHERE [OrganizationId]=@orgId AND [IsActive]=1
            ORDER BY [Name]")
        {
        }

        public int OrgId { get; set; }
    }
}