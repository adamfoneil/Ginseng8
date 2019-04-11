namespace Ginseng.Models.Interfaces
{
	public interface INotifyOptions
	{
		int Id { get; }
		string TableName { get; }
		bool SendEmail { get; }
		bool SendText { get; }
		bool InApp { get; }
	}
}