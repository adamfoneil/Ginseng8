namespace Ginseng.Mvc.Interfaces
{
	public interface IWorkItemTitle
	{
		int Number { get; }
        string TeamName { get; }
        int ApplicationId { get; }
        string ApplicationName { get; }
        int ProjectId { get; }
        string DisplayProjectName { get; }
		string Title { get; }
		bool IsEditable(string userName);
	}
}