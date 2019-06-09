using Ginseng.Models.Conventions;
using Postulate.Base.Attributes;
using System;

namespace Ginseng.Models
{
    /// <summary>
    /// Associates a developer with a milestone in order to determine her/his available hours and feasibility of work items
    /// </summary>
    public class DeveloperMilestone : BaseTable
    {
        [References(typeof(UserProfile))]
        [PrimaryKey]
        public int DeveloperId { get; set; }

        [References(typeof(Milestone))]
        [PrimaryKey]
        public int MilestoneId { get; set; }

        public DateTime StartDate { get; set; }
    }
}
