using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Ginseng.Models
{
	[TrackChanges(IgnoreProperties = "DateModified,ModifiedBy")]
	public class Article : BaseTable, IBody
	{
		[References(typeof(Organization))]
		[PrimaryKey]
		public int OrganizationId { get; set; }

		[MaxLength(255)]
		[PrimaryKey]
		public string Title { get; set; }

		[MaxLength(255)]
		public string Location { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		public bool IsActive { get; set; } = true;
	}
}