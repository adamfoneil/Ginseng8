using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class DataModels : Query<DataModel>
	{
		public DataModels() : base("SELECT * FROM [dbo].[DataModel] WHERE [IsActive]=@isActive {andWhere} ORDER BY [Name]")
		{
		}

		[Where("[ApplicationId]=@appId")]
		public int? AppId { get; set; }

		[Where("EXISTS(SELECT 1 FROM [dbo].[Application] [app] INNER JOIN [dbo].[DataModel] [dm] ON [app].[Id]=[dm].[ApplicationId] WHERE [app].[OrganizationId]=@orgId)")]
		public int? OrgId { get; set; }

		public bool IsActive { get; set; }
	}
}