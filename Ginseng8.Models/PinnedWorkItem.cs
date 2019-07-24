using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
    public class PinnedWorkItem : BaseTable
    {
        public const string PinnedIcon = "fas fa-thumbtack";
        public const string UnpinnedIcon = "fal fa-thumbtack";

        [References(typeof(UserProfile))]
        [PrimaryKey]
        public int UserId { get; set; }

        [References(typeof(WorkItem))]
        [PrimaryKey]
        public int WorkItemId { get; set; }        
    }
}
