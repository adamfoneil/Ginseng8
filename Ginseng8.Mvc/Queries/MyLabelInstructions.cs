using Ginseng.Models;
using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
    public class MyLabelInstructions : Query<LabelInstructions>
    {
        public MyLabelInstructions() : base(
            @"SELECT [li].*
            FROM [dbo].[LabelInstructions] [li]
            INNER JOIN [dbo].[Label] [lbl] ON [li].[LabelId]=[lbl].[Id]
            WHERE [lbl].[OrganizationId]=@orgId")
        {
        }

        public int OrgId { get; set; }
    }
}