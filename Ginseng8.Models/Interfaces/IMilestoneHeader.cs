using System;

namespace Ginseng.Models.Interfaces
{
    public interface IMilestoneHeader
    {
        int MilestoneId { get; }
        string MilestoneName { get; }
        DateTime? MilestoneDate { get; }
        int? MilestoneDaysAway { get; }
        bool IsMilestoneVisible { get; }        
    }
}
