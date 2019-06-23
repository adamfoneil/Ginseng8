using Ginseng.Models.Interfaces;
using System.Collections.Generic;

namespace Ginseng.Mvc.ViewModels
{
    /// <summary>
    /// Backs the UI for dropdown multi-select elements (e.g. Setup/Labels)
    /// </summary>
    public class MultiSelector<T> where T : ISelectable
    {
        public string IdPrefix { get; set; }
        public string Prompt { get; set; }
        public string PrimaryFieldName { get; set; }
        public string RelatedFieldName { get; set; }
        public int RelatedId { get; set; }
        public IEnumerable<T> Items { get; set; }
        public string PostUrl { get; set; }
    }
}