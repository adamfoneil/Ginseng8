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
		public const string ForwardHandOff = "fas fa-chevron-circle-right";
		public const string BackwardHandOff = "far fa-chevron-circle-left";

		public static string GetIconClass(bool isForward)
		{
			return (isForward) ? ForwardHandOff : BackwardHandOff;
		}

		public static string GetColor(bool isForward)
		{
			return (isForward) ? "green" : "orange";
		}

		[References(typeof(WorkItem))]
		public int WorkItemId { get; set; }

		public string IconClass => (IsForward) ? ForwardHandOff : BackwardHandOff;

		public int FromUserId { get; set; }

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