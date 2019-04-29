namespace Ginseng.Mvc.Interfaces
{
	public interface IWorkItemTitle
	{
		int Number { get; }
		int ProjectId { get; }
		string ProjectName { get; }
		int? ProjectPriority { get; }
		string Title { get; }
		string CreatedBy { get; }
	}
}