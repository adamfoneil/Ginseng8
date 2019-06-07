using Ginseng.Models;
using Ginseng.Models.Interfaces;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
    /// <summary>
    /// Backs the UI for dropdown multi-select elements (e.g. Setup/Labels)
    /// </summary>    
    public class MultiSelector<T> where T : ISelectable
    {
        public int RelatedId { get; set; }
        public IEnumerable<T> Items { get; set; }        
    }
}