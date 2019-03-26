using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	/// <summary>
	/// Collection of model classes (database tables) grouped under a name
	/// </summary>
	[TrackChanges(IgnoreProperties = "DateModified,ModifiedBy")]
	public class DataModel : BaseTable, IBody
	{
		[References(typeof(Application))]
		[PrimaryKey]
		public int ApplicationId { get; set; }

		[MaxLength(50)]
		[PrimaryKey]
		public string Name { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		public bool IsActive { get; set; } = true;
	}
}