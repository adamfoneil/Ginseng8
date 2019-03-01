using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class MyOrgSelect : SelectListQuery
	{
		public MyOrgSelect() : base(
			@"SELECT 
				[Id] AS [Value], [Name] AS [Text]
			FROM 
				[dbo].[Organization] [org]
			WHERE
				[org].[OwnerUserId]=@userId OR
				EXISTS(SELECT 1 FROM [dbo].[OrganizationUser] WHERE [OrganizationId]=[org].[Id] AND [UserId]=@userId AND [IsEnabled]=1)")
		{
		}

		public int UserId { get; set; }
	}
}