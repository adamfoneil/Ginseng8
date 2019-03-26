using Ginseng.Mvc.Classes;

namespace Ginseng.Mvc.Queries.SelectLists
{
	public class DataModelSelect : SelectListQuery
	{
		public DataModelSelect() : base(
			@"SELECT [Id] AS [Value], [Name] AS [Text]
			FROM [dbo].[DataModel] [dm]
			WHERE [ApplicationId]=@appId AND [IsActive]=1
			ORDER BY [Name]")
		{
		}

		public int AppId { get; set; }
	}
}