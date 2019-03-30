using Dapper;
using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Threading.Tasks;

namespace Ginseng.Models
{
	/// <summary>
	/// Description of work scope and estimate for a work item, and the assignment of a developer to a task
	/// </summary>
	public class WorkItemDevelopment : BaseTable, IFeedItem, IBody
	{
		[References(typeof(WorkItem))]
		[PrimaryKey]
		public int WorkItemId { get; set; }

		public string IconClass => "tools";

		[MaxLength(255)]
		public string BranchUrl { get; set; }

		/// <summary>
		/// Item-specific estimate (overrides WorkItemSize.EstimateHours)
		/// </summary>
		public int? EstimateHours { get; set; }

		public string HtmlBody { get; set; }

		public string TextBody { get; set; }

		[NotMapped]
		public int OrganizationId { get; set; }

		public async Task SetOrganizationIdAsync(IDbConnection connection)
		{
			OrganizationId = await WorkItem.GetOrgIdAsync(connection, WorkItemId);
		}
	}
}