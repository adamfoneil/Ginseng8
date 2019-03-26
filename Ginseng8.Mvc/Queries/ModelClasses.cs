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
			WHERE [mc].[DataModelId]=@modelId {andWhere}
			ORDER BY [mc].[Name]")
		{
		}

		public int ModelId { get; set; }		

		[Where("[mc].[IsScalarType]=@isScalar")]
		public bool? IsScalar { get; set; }
	}
}