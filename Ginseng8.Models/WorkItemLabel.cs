using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
    /// <summary>
    /// Associates a work item and a label
    /// </summary>
    public class WorkItemLabel : BaseTable
    {
        [References(typeof(WorkItem))]
        [PrimaryKey]
        public int WorkItemId { get; set; }

        [References(typeof(Label))]
        [PrimaryKey]
        public int LabelId { get; set; }
    }
}