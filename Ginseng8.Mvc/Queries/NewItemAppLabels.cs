using System.Collections.Generic;
using System.Data;
using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class NewItemAppLabels : Query<Label>, ITestableQuery
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

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new NewItemAppLabels() { OrgId = 1 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}