namespace Ginseng.Models.Interfaces
{
    public interface INotifyOptions
    {
        int Id { get; }
        string TableName { get; }
        bool SendEmail { get; }
        bool SendText { get; }
        bool InApp { get; }

        bool AllowNotification();
    }

    /// <summary>
    /// default implementation hack
    /// </summary>
    public static class NotifyOptionsImplementation
    {
        public static bool AllowNotification(INotifyOptions options)
        {
            return options.SendEmail || options.SendText || options.InApp;
        }
    }
}