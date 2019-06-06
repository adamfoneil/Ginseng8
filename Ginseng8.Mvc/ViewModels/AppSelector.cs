using Ginseng.Models;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
    public class AppSelector
    {
        public int RelatedId { get; set; }
        public IEnumerable<Application> Applications { get; set; }        
    }
}