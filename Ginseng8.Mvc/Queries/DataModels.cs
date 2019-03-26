using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class DataModels : Query<DataModel>
	{
		public DataModels() : base("SELECT * FROM [dbo].[DataModel] WHERE [IsActive]=@isActive {andWhere} ORDER BY [Name]")
		{
		}

		[Where("[ApplicationId]=@appId")]
		public int? AppId { get; set; }

		public bool IsActive { get; set; }
	}
}