using Ginseng.Models.Interfaces;

namespace Ginseng.Mvc.ViewModels
{
    public class MilestoneHeaderView
    {
        public MilestoneHeaderView(IMilestoneHeader header, bool allowToggle = false)
        {
            Header = header;
            AllowToggle = allowToggle;
        }

        public IMilestoneHeader Header { get; set; }
        public bool AllowToggle { get; set; }
    }
}
