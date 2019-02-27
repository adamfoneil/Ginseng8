using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
	/// <summary>
	/// Indicates that development work is done, development is seeking business sign-off on changes.
	/// Contains testing instructions and any info that helps business understand what was done
	/// </summary>
	public class ApprovalRequest : BaseTable, IBody, IFeedItem
	{
		[PrimaryKey]
		[References(typeof(WorkItem))]
		public int WorkItemId { get; set; }

		public string IconClass => "signoff";

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }
	}
}