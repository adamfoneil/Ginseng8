using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages.Setup
{
    public class GitHubModel : AppPageModel
    {
		public GitHubModel(IConfiguration config) : base(config)
		{
		}

        public void OnGet()
        {

        }
    }
}