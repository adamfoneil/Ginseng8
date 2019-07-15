using Ginseng.Models;
using Ginseng.Mvc.ViewModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Setup
{
    public class OptionsModel : AppPageModel
    {
        public OptionsModel(IConfiguration config) : base(config)
        {
        }
        
        public object GetFieldModel(UserOptionValue userOptionValue)
        {
            throw new NotImplementedException();

            switch (userOptionValue.TypeId)
            {
                //case 
            }
        }



        public async Task OnGetAsync()
        {

        }

        private class OptionUIInfo
        {
            public string ViewName { get; set; }
        }
    }
}