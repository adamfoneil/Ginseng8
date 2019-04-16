namespace Ginseng.Mvc.Interfaces
{
	public interface IWorkItemTitle
	{
		int ProjectId { get; }
		string ProjectName { get; }
		int? ProjectPriority { get; }
		string Title { get; }
	}
}