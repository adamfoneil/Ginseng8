namespace Ginseng.Mvc.Interfaces
{
    public interface IItemMetrics
    {
        bool HasModifiers();

        int? TotalWorkItems { get; }
        int? OpenWorkItems { get; set; }
        int? EstimateHours { get; set; }
        int? UnestimatedWorkItems { get; set; }
        int? StoppedWorkItems { get; set; }
        int? ImpededWorkItems { get; set; }
        double PercentComplete { get; set; }
    }
}