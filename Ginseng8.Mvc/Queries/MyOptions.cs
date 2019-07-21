using System.Collections.Generic;
using System.Data;
using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Interfaces;

namespace Ginseng.Mvc.Queries
{
    public class MyOptions : Query<UserOptionValue>, ITestableQuery
    {
        public MyOptions() : base(
            @"SELECT [uov].*, [ot].[StorageColumn], [o].[Name] AS [OptionName], [o].[TypeId]
            FROM [dbo].[UserOptionValue] [uov]
            INNER JOIN [app].[Option] [o] ON [uov].[OptionId]=[o].[Id]
            INNER JOIN [app].[OptionType] [ot] ON [o].[TypeId]=[ot].[Id]
            WHERE [UserId]=@userId
            UNION
            SELECT [uov].*, [ot].[StorageColumn], [o].[Name] AS [OptionName], [o].[TypeId]
            FROM [dbo].[UserOptionValue] [uov]
            INNER JOIN [app].[Option] [o] ON [uov].[OptionId]=[o].[Id]
            INNER JOIN [app].[OptionType] [ot] ON [o].[TypeId]=[ot].[Id]
            WHERE [UserId]=0 AND NOT EXISTS(SELECT 1 FROM [dbo].[UserOptionValue] WHERE [UserId]=@userId AND [OptionId]=[o].[Id])")
        {
        }

        public int UserId { get; set; }

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new MyOptions() {  UserId = 1 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}
