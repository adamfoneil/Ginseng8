using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ginseng.Models
{
	/// <summary>
	/// Describes high-level business objectives for an application at a given milestone
	/// </summary>
	public class AppMilestone : BaseTable, IBody
	{
		[References(typeof(Application))]
		[PrimaryKey]
		public int ApplicationId { get; set; }

		[References(typeof(Milestone))]
		[PrimaryKey]
		public int MilestoneId { get; set; }

		public string TextBody { get; set; }

		public string HtmlBody { get; set; }

		[NotMapped]
		public string ApplicationName { get; set; }
	}
}