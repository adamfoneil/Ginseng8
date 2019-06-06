using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class NewItemAppLabels : Query<NewItemAppLabel>
    {
        public NewItemAppLabels() : base(
            @"SELECT [nal].*, [app].[Name] AS [ApplicationName]
            FROM [dbo].[NewItemAppLabel] [nal]
            INNER JOIN [dbo].[Application] [app] ON [nal].[ApplicationId]=[app].[Id]
            WHERE [app].[OrganizationId]=@orgId")
        {
        }

        public int OrgId { get; set; }
    }
}