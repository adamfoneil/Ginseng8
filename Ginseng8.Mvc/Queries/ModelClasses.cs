using Ginseng.Models;
using Postulate.Base;
using Postulate.Base.Attributes;

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