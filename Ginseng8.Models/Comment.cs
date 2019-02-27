using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
	/// <summary>
	/// Info added to a work item
	/// </summary>
	public class Comment : BaseTable, IFeedItem, IBody
	{
		[References(typeof(WorkItem))]
		public int WorkItemId { get; set; }

		public string IconClass => "comment";

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }
	}
}