using Ginseng.Models;
using Dapper.QX;
using Dapper.QX.Attributes;

namespace Ginseng.Mvc.Queries
{
	public class ModelClasses : Query<ModelClass>
	{
		public ModelClasses() : base(
			@"SELECT [mc].*
			FROM [dbo].[ModelClass] [mc]
			{where}
			ORDER BY [mc].[Name]")
		{
		}

		[Where("[mc].[DataModelId]=@modelId")]
		public int? ModelId { get; set; }		

		[Where("[mc].[IsScalarType]=@isScalar")]
		public bool? IsScalar { get; set; }
	}
}