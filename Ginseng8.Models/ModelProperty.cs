using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	/// <summary>
	/// Describes a model property (database column)
	/// </summary>
	[TrackChanges(IgnoreProperties = "DateModified,ModifiedBy")]
	public class ModelProperty : BaseTable
	{
		[References(typeof(ModelClass))]
		[PrimaryKey]
		public int ModelClassId { get; set; }

		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		[References(typeof(ModelClass))]
		public int TypeId { get; set; }

		public bool InPrimaryKey { get; set; }

		public bool IsNullable { get; set; }

		public string Description { get; set; }

		public int? Position { get; set; }
	}
}