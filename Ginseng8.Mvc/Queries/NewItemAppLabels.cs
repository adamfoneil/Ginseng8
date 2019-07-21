using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
    public class NewItemAppLabels : Query<Label>
    {
        public NewItemAppLabels() : base(
            @"SELECT 
                [nal].[ApplicationId],
                [lbl].*                
            FROM
                [dbo].[NewItemAppLabel] [nal]
                INNER JOIN [dbo].[Label] [lbl] ON [nal].[LabelId]=[lbl].[Id]
            WHERE 
                [lbl].[OrganizationId]=@orgId")
        {
        }

        public int OrgId { get; set; }
    }
}