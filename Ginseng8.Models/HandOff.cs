using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
	/// <summary>
	/// Test instructons or feedback, request for code review, or another other change in activity on a work item
	/// </summary>
	public class HandOff : BaseTable, IFeedItem, IBody
	{
		[References(typeof(WorkItem))]
		public int WorkItemId { get; set; }

		public string IconClass => (IsForward) ? "angle-double-right" : "angle-double-left";

		[References(typeof(Activity))]
		public int FromActivityId { get; set; }

		[References(typeof(Activity))]
		public int ToActivityId { get; set; }

		/// <summary>
		/// Indicates whether the from and to activities move forward through workflow
		/// </summary>
		public bool IsForward { get; set; }

		public string HtmlBody { get; set; }

		public string TextBody { get; set; }
	}
}