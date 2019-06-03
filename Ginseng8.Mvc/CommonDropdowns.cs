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
		public ILookup<int, SelectListItem> AllProjects { get; set; }
		public IEnumerable<SelectListItem> Sizes { get; set; }
		public IEnumerable<SelectListItem> CloseReasons { get; set; }		
        public ILookup<int, SelectListItem> AllMilestones { get; set; }
		public ILookup<int, SelectListItem> AllDataModels { get; set; }
		public IEnumerable<Label> Labels { get; set; }

		public SelectList AppSelect(int? appId)
		{
			return new SelectList(Applications, "Value", "Text", appId);
		}

		public SelectList AppSelect(OpenWorkItemsResult item = null)
		{
			return new SelectList(Applications, "Value", "Text", item?.ApplicationId);
		}

		public SelectList ProjectSelect(int appId, int? projectId, bool withNoneOption = false)
		{
			var items = AllProjects[appId].ToList();
			if (withNoneOption) items.Insert(0, new SelectListItem() { Value = "0", Text = "- has no project -" });
			return new SelectList(items, "Value", "Text", projectId);
		}

		public SelectList ProjectSelect(OpenWorkItemsResult item = null)
		{
			return new SelectList(AllProjects[item?.ApplicationId ?? 0], "Value", "Text", item?.ProjectId);
		}

		public SelectList DataModelSelect(int appId, int? dataModelId)
		{
			return new SelectList(AllDataModels[appId], "Value", "Text", dataModelId);
		}

		public SelectList SizeSelect(int? sizeId, bool withNoneOption = false)
		{
			var items = Sizes.ToList();
			if (withNoneOption) items.Insert(0, new SelectListItem() { Value = "0", Text = "- has no size -" });
			return new SelectList(items, "Value", "Text", sizeId);
		}

		public SelectList SizeSelect(OpenWorkItemsResult item = null)
		{
			return new SelectList(Sizes, "Value", "Text", item?.SizeId);
		}

		public SelectList CloseReasonSelect(OpenWorkItemsResult item = null)
		{
			return new SelectList(CloseReasons, "Value", "Text", item?.CloseReasonId);
		}

		public SelectList MilestoneSelect(int appId, int? milestoneId, bool withNoneOption = false)
		{
			var items = AllMilestones[appId].ToList();
			if (withNoneOption) items.Insert(0, new SelectListItem() { Value = "0", Text = "- has no milestone -" });
			return new SelectList(items, "Value", "Text", milestoneId);
		}

		public SelectList MilestoneSelect(OpenWorkItemsResult item = null)
		{
			return new SelectList(AllMilestones[item?.ApplicationId ?? 0], "Value", "Text", item?.MilestoneId);
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

            // org-scoped items
			result.Activities = await new Activities() { OrgId = orgId, IsActive = true }.ExecuteAsync(connection);
            result.Sizes = await new SizeSelect() { OrgId = orgId }.ExecuteAsync(connection);
            result.CloseReasons = await new CloseReasonSelect().ExecuteAsync(connection);
            result.Applications = await new AppSelect() { OrgId = orgId }.ExecuteAsync(connection);
            result.Labels = await new Labels() { OrgId = orgId, IsActive = true }.ExecuteAsync(connection);

            // app-scoped items
            var allProjects = await new AllProjects() { OrgId = orgId }.ExecuteAsync(connection);
			result.AllProjects = allProjects.ToLookup(row => row.ApplicationId, row => row.ToSelectListItem());

            var allMilestones = await new AllMilestones() { OrgId = orgId }.ExecuteAsync(connection);
            result.AllMilestones = allMilestones.ToLookup(row => row.ApplicationId, row => row.ToSelectListItem());
                       
			var allModels = await new DataModels() { OrgId = orgId, IsActive = true }.ExecuteAsync(connection);
			result.AllDataModels = allModels.ToLookup(row => row.ApplicationId, row => new SelectListItem() { Value = row.Id.ToString(), Text = row.Name });

			return result;
		}
	}
}