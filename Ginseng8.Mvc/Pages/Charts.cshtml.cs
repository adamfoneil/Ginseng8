using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace Ginseng.Mvc.Pages
{
    public class ChartsModel : AppPageModel
    {
        public ChartsModel(IConfiguration config) : base(config)
        {
        }

        public async Task OnGetAsync()
        {

        }
    }
}