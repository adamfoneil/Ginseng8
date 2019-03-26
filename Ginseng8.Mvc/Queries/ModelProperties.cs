using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class ModelProperties : Query<ModelProperty>
	{
		public ModelProperties() : base(
			@"SELECT [mp].*
			FROM [dbo].[ModelProperty] [mp]
			WHERE [mp].[ModelClassId]=@classId
			ORDER BY [Position], [Name]")
		{
		}

		public int ClassId { get; set; }
	}
}