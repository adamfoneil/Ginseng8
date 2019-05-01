using Ginseng.Models.Freshdesk;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Tickets
{
	[Authorize]
	public class IndexModel : AppPageModel
	{
		private FreshdeskTicketCache _cache;

		public IndexModel(IConfiguration config) : base(config)
		{			
			_cache = new FreshdeskTicketCache(config);
		}

		public IEnumerable<Ticket> Tickets { get; set; }
		public LoadedFrom LoadedFrom { get; set; }
		public DateTime DateQueried { get; set; }

		public async Task OnGetAsync()
		{
			Tickets = await _cache.QueryAsync(Data.CurrentOrg.Name);
			LoadedFrom = _cache.LoadedFrom;
			DateQueried = _cache.LastApiCallDateTime;
		}
	}
}