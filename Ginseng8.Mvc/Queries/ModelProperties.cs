using Ginseng.Models;
using Postulate.Base;

namespace Ginseng.Mvc.Queries
{
	public class ModelProperties : Query<ModelProperty>
	{
		public ModelProperties() : base(
			@"SELECT [mp].*, [mc].[IsScalarType]
			FROM [dbo].[ModelProperty] [mp]
			INNER JOIN [dbo].[ModelClass] [mc] ON [mp].[TypeId]=[mc].[Id]
			WHERE [mp].[ModelClassId]=@classId
			ORDER BY [Position], [Name]")
		{
		}

		public int ClassId { get; set; }
	}
}