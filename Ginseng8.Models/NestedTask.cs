using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
    public class NestedTask : BaseTable
    {
        [References(typeof(WorkItem))]
        [PrimaryKey]
        public int WorkItemId { get; set; }

        [PrimaryKey]
        public int Index { get; set; }

        public bool IsChecked { get; set; }
    }
}
