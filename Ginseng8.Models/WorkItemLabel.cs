using System.Data;
using System.Threading.Tasks;
using Ginseng.Models.Conventions;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;

namespace Ginseng.Models
{
    /// <summary>
    /// Associates a work item and a label
    /// </summary>
    public class WorkItemLabel : BaseTable
    {
        public const string IconClass = "fal fa-tags";
        public const string IconColor = "black";

        [References(typeof(WorkItem))]
        [PrimaryKey]
        public int WorkItemId { get; set; }

        [References(typeof(Label))]
        [PrimaryKey]
        public int LabelId { get; set; }

        public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action, IUser user)
        {
            if (action == SaveAction.Insert)
            {
                var workItem = await connection.FindAsync<WorkItem>(WorkItemId);
                var label = await connection.FindAsync<Label>(LabelId);
                string displayUser = await OrganizationUser.GetUserDisplayNameAsync(connection, workItem.OrganizationId, user.UserName);
                string text = $"{displayUser} added label {label.Name} to work item {workItem.Number}";
                string html = $"{displayUser} added label <strong>{label.Name}</strong> to work item {workItem.Number}";

                int eventLogId = await EventLog.WriteAsync(connection, new EventLog()
                {
                    OrganizationId = workItem.OrganizationId,
                    ApplicationId = workItem.ApplicationId,
                    WorkItemId = workItem.Id,
                    EventId = SystemEvent.LabelAdded,
                    IconClass = IconClass,
                    IconColor = IconColor,
                    HtmlBody = html,
                    TextBody = text,
                    SourceId = Id,
                    SourceTable = nameof(WorkItemLabel)
                }, user);

                await Notification.CreateFromLabelSubscriptions(connection, eventLogId);
            }
        }
    }
}