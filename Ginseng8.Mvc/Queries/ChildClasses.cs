using Dapper.QX;

namespace Ginseng.Mvc.Queries
{
	public enum RelationshipType
	{
		OneToOne,
		OneToMany
	}

	public class ChildClassesResult
	{
		public int ClassId { get; set; }
		public string ClassName { get; set; }
		public string PropertyName { get; set; }
		public bool InPrimaryKey { get; set; }
		public int KeyColumnCount { get; set; }

		public RelationshipType RelationshipType		
		{
			get
			{
				return (InPrimaryKey && KeyColumnCount == 1) ? RelationshipType.OneToOne : RelationshipType.OneToMany;
			}			
		}
	}

	public class ChildClasses : Query<ChildClassesResult>
	{
		public ChildClasses() : base(
			@"SELECT
				[child].[Id] AS [ClassId],
				[child].[Name] AS [ClassName],
				[mp].[Name] AS [PropertyName],
				[mp].[InPrimaryKey],
				(SELECT COUNT(1) FROM [dbo].[ModelProperty] WHERE [ModelClassId]=[child].[Id] AND [InPrimaryKey]=1) AS [KeyColumnCount]
			FROM
				[dbo].[ModelProperty] [mp]
				INNER JOIN [dbo].[ModelClass] [parent] ON [mp].[TypeId]=[parent].[Id]
				INNER JOIN [dbo].[ModelClass] [child] ON [mp].[ModelClassId]=[child].[Id]
			WHERE
				[parent].[Id]=@parentClassId")
		{
		}

		public int ParentClassId { get; set; }
	}
}