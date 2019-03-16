using Ginseng.Models;
using Ginseng.Mvc.Queries;
using Ginseng.Mvc.Queries.SelectLists;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc
{
	public class CommonDropdowns
	{
		private CommonDropdowns()
		{
		}

		/// <summary>
		/// Activities are handled differently from the SelectList properties
		/// </summary>
		public IEnumerable<Activity> Activities { get; set; }

		public IEnumerable<SelectListItem> Applications { get; set; }
		public IEnumerable<SelectListItem> Projects { get; set; }		
		public IEnumerable<SelectListItem> Sizes { get; set; }
		public IEnumerable<SelectListItem> CloseReasons { get; set; }
		public IEnumerable<SelectListItem> Milestones { get; set; }			
		public IEnumerable<Label> Labels { get; set; }

		public SelectList AppSelect(OpenWorkItemsResult item)
		{
			return new SelectList(Applications, "Value", "Text", item.ApplicationId);
		}

		public SelectList ProjectSelect(OpenWorkItemsResult item)
		{
			return new SelectList(Projects, "Value", "Text", item.ProjectId);
		}

		public SelectList SizeSelect(OpenWorkItemsResult item)
		{
			return new SelectList(Sizes, "Value", "Text", item.SizeId);
		}

		public SelectList CloseReasonSelect(OpenWorkItemsResult item)
		{
			return new SelectList(CloseReasons, "Value", "Text", item.CloseReasonId);
		}

		public SelectList MilestoneSelect(OpenWorkItemsResult item)
		{
			return new SelectList(Milestones, "Value", "Text", item.MilestoneId);
		}

		public IEnumerable<Label> LabelItems(IEnumerable<Label> selectedLabels)
		{			
			return Labels.Select(lbl => new Label()
			{
				Id = lbl.Id,
				Name = lbl.Name,
				BackColor = lbl.BackColor,
				ForeColor = lbl.ForeColor,
				Selected = selectedLabels.Contains(lbl)
			});
		}

		public static async Task<CommonDropdowns> FillAsync(SqlConnection connection, int orgId, int responsibilities)
		{
			var result = new CommonDropdowns();

			result.Activities = await new Activities() { OrgId = orgId, IsActive = true }.ExecuteAsync(connection);

			result.Applications = await new AppSelect() { OrgId = orgId }.ExecuteAsync(connection);
			result.Projects = await new ProjectSelect() { OrgId = orgId }.ExecuteAsync(connection);			
			result.Sizes = await new SizeSelect() { OrgId = orgId }.ExecuteAsync(connection);
			result.CloseReasons = await new CloseReasonSelect().ExecuteAsync(connection);
			result.Milestones = await new MilestoneSelect() { OrgId = orgId }.ExecuteAsync(connection);
			result.Labels = await new Labels() { OrgId = orgId, IsActive = true }.ExecuteAsync(connection);
			return result;
		}
	}
}