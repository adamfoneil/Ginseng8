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
	public class ModelProperty : BaseTable, IBody
	{
		[References(typeof(ModelClass))]
		[PrimaryKey]
		public int ModelClassId { get; set; }

		[PrimaryKey]
		[MaxLength(50)]
		public string Name { get; set; }

		[References(typeof(ModelClass))]
		public int TypeId { get; set; }

		public bool IsPrimaryKey { get; set; }

		public bool IsNullable { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		public int Position { get; set; }
	}
}