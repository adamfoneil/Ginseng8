using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
    public class PinnedWorkItem : BaseTable
    {
        [References(typeof(UserProfile))]
        [PrimaryKey]
        public int UserId { get; set; }

        [References(typeof(WorkItem))]
        [PrimaryKey]
        public int WorkItemId { get; set; }
    }
}
