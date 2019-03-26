using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class ModelClasses : Query<ModelClass>
	{
		public ModelClasses() : base(
			@"SELECT [mc].*
			FROM [dbo].[ModelClass] [mc]
			WHERE [mc].[DataModelId]=@modelId
			ORDER BY [mc].[Name]")
		{
		}

		public int ModelId { get; set; }		
	}
}