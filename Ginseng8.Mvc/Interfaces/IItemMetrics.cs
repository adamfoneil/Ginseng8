namespace Ginseng.Mvc.Interfaces
{
    public interface IItemMetrics
    {
        bool HasModifiers();

        int? TotalWorkItems { get; }
        int? OpenWorkItems { get; }
        int? EstimateHours { get; }
        int? UnestimatedWorkItems { get; }
        int? StoppedWorkItems { get; }
        int? ImpededWorkItems { get; }
        double PercentComplete { get; }
    }
}