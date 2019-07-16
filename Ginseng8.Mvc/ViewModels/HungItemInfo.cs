using Ginseng.Mvc.Queries;
using System.Collections.Generic;
using System.Linq;

namespace Ginseng.Mvc.ViewModels
{
    public class HungItemInfo
    {
        public string Icon { get; set; }
        public string Color { get; set; }
        public HungReason Reason { get; set; }
        public string Text { get; set; }      
        public string Description { get; set; }
        public int Count { get; set; } // set at runtime to whatever count is appropriate for the view

        public static IEnumerable<HungItemInfo> GetValues()
        {
            yield return new HungItemInfo()
            {
                Reason = HungReason.HasImpediment,                
                Text = "Has Impediment",
                Icon = OpenWorkItemsResult.ImpedimentIcon,
                Color = OpenWorkItemsResult.ImpedimentColor,
                Description = "A technical issue is preventing progress."
            };

            yield return new HungItemInfo()
            {
                Reason = HungReason.IsPaused,
                Text = "Paused",
                Icon = OpenWorkItemsResult.PausedIcon,
                Color = "auto",
                Description = "Items waiting for someone to resume, either because of hand-off or the assigned people removed themselves."
            };

            yield return new HungItemInfo()
            {
                Reason = HungReason.IsStopped,
                Text = "Not Assigned",
                Icon = OpenWorkItemsResult.StoppedIcon,
                Color = OpenWorkItemsResult.StoppedColor,
                Description = "Items in a milestone that no one is working on."
            };
        }

        public static Dictionary<HungReason, HungItemInfo> Dictionary
        {
            get { return GetValues().ToDictionary(row => row.Reason); }
        }
    }
}
