using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
    /// <summary>
    /// Lets users set their own order for work order priority, used with WorkDay ordering
    /// </summary>
    public class WorkItemUserPriority : BaseTable
    {
        [References(typeof(WorkItem))]
        [PrimaryKey]
        public int WorkItemId { get; set; }

        [References(typeof(UserProfile))]
        [PrimaryKey]
        public int UserId { get; set; }

        /// <summary>
        /// Day index (where 0 = today, +1 = tomorrow, +2 = day after tomorrow, etc) when you want to work on this
        /// </summary>
        public int WorkDay { get; set; }

        public int Value { get; set; }
    }
}
