using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
	/// <summary>
	/// Test instructions and sign-off
	/// </summary>
	public class TestWork : BaseTable, IBody, IFeedItem
	{
		[PrimaryKey]
		[References(typeof(WorkItem))]
		public int WorkItemId { get; set; }

		public string IconClass => "test";

		/// <summary>
		/// Business person who owns this item
		/// </summary>
		[References(typeof(UserProfile))]
		public int UserId { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		public bool IsDone { get; set; }
	}
}