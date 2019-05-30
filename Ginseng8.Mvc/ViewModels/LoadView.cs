using Ginseng.Mvc.Queries;

namespace Ginseng.Mvc.ViewModels
{
	public class LoadView
	{
		public int EstimateHours { get; set; }
		public int WorkHours { get; set; }
        public MilestoneMetricsResult MilestoneMetrics { get; set; }

		public int Overload
		{
			get { return EstimateHours - WorkHours; }
		}

		public string WorkHoursStyle
		{
			get { return (Overload > 0) ? "danger" : "muted"; }
		}
	}
}