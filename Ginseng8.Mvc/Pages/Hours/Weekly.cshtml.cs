using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Hours
{
    public class WeeklyModel : AppPageModel
    {
		public WeeklyModel(IConfiguration config) : base(config)
		{
		}

		//public IEnumerable<WorkItem>

        public async Task OnGetAsync()
        {

        }
    }
}