﻿using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ginseng.Mvc.Models.Freshdesk.Dto;

namespace Ginseng.Mvc.Pages.Tickets
{
	[Authorize]
	public class IndexModel : AppPageModel
    {
        private readonly FreshdeskTicketCache _cache;

		public IndexModel(
            IConfiguration config,
            FreshdeskTicketCache cache) 
            : base(config)
        {
            _cache = cache;
        }

		public IEnumerable<Ticket> Tickets { get; set; }
		public LoadedFrom LoadedFrom { get; set; }
		public DateTime DateQueried { get; set; }
        public string FreshdeskUrl { get; set; }

		public async Task OnGetAsync()
		{
            FreshdeskUrl = Data.CurrentOrg.FreshdeskUrl;
            Tickets = await _cache.QueryAsync(Data.CurrentOrg.Name);
			LoadedFrom = _cache.LoadedFrom;
			DateQueried = _cache.LastApiCallDateTime;
		}
	}
}