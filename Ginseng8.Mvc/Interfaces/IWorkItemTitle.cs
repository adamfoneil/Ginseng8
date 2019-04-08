namespace Ginseng.Mvc.Interfaces
{
	public interface IWorkItemTitle
	{
		int ProjectId { get; }
		string ProjectName { get; }
		string Title { get; }
	}
}