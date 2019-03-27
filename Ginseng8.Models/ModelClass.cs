using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
	/// <summary>
	/// Describes a database table
	/// </summary>
	[TrackChanges(IgnoreProperties = "DateModified,ModifiedBy")]
	public class ModelClass : BaseTable, IBody
	{
		[References(typeof(DataModel))]
		[PrimaryKey]
		public int DataModelId { get; set; }

		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }

		[MaxLength(50)]
		public string ObjectName { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		/// <summary>
		/// If true, then class has no properties (used for "built-in" types like int, money, date, etc)
		/// </summary>
		public bool IsScalarType { get; set; }	
	}
}