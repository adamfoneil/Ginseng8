using Ginseng.Mvc.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Work
{
	[Authorize]
	public class AllItemsModel : DashboardPageModel
	{
		public AllItemsModel(IConfiguration config) : base(config)
		{
		}

		[BindProperty(SupportsGet = true)]
		public string Query { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? FilterProjectId { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? FilterMilestoneId { get; set; }

		[BindProperty(SupportsGet = true)]
		public int? FilterSizeId { get; set; }

		protected override OpenWorkItems GetQuery()
		{
			return new OpenWorkItems()
			{
				OrgId = OrgId,
				AppId = CurrentOrgUser.CurrentAppId,
				ProjectId = FilterProjectId,
				LabelId = LabelId,				
				MilestoneId = FilterMilestoneId,
				SizeId = FilterSizeId,
				TitleAndBodySearch = Query
			};
		}
	}
}