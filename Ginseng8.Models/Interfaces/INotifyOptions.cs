namespace Ginseng.Models.Interfaces
{
	public interface INotifyOptions
	{
		int Id { get; set; }
		bool SendEmail { get; set; }
		bool SendText { get; set; }
		bool InApp { get; set; }
	}
}