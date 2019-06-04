namespace Ginseng.Mvc.Interfaces
{
    public interface IItemMetrics
    {
        bool HasModifiers();

        int? UnestimatedWorkItems { get; set; }
        int? StoppedWorkItems { get; set; }
        int? ImpededWorkItems { get; set; }
    }
}