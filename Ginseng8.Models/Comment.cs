using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Ginseng.Models.Queries;
using Html2Markdown;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    public enum ObjectType
    {
        WorkItem = 1,
        Project = 2,
        DataModel = 3,
        ModelClass = 4,
        Article = 5
    }

    /// <summary>
    /// Info added to a work item
    /// </summary>
    public class Comment : BaseTable, IBody, IOrgSpecific
    {
        public const string CommentIcon = "far fa-comment";
        public const string ImpedimentIcon = "fas fa-comment-times";
        public const string ResolvedIcon = "fas fa-comment-check";

        [References(typeof(Organization))]
        public int OrganizationId { get; set; }

        public ObjectType ObjectType { get; set; } = ObjectType.WorkItem;

        public int ObjectId { get; set; }

        public bool? IsImpediment { get; set; }

        public string IconClass => (!IsImpediment.HasValue) ?
            CommentIcon :
                (IsImpediment.Value) ?
                    ImpedimentIcon :
                    ResolvedIcon;

        public string TextBody { get; set; }

        public string HtmlBody { get; set; }

        [NotMapped]
        public string DisplayName { get; set; }

        public override async Task AfterSaveAsync(IDbConnection connection, SaveAction action, IUser user)
        {
            await base.AfterSaveAsync(connection, action, user);

            if (action == SaveAction.Insert)
            {
                if (ObjectType == ObjectType.WorkItem)
                {
                    var workItem = await connection.FindAsync<WorkItem>(ObjectId);
                    if (IsImpediment.HasValue)
                    {
                        workItem.HasImpediment = IsImpediment.Value;
                        await connection.UpdateAsync(workItem, user, r => r.HasImpediment);
                    }

                    await EventLog.WriteAsync(connection, new EventLog(ObjectId, user)
                    {
                        TeamId = workItem.TeamId,
                        EventId = (IsImpediment ?? false) ? SystemEvent.ImpedimentAdded : SystemEvent.CommentAdded,
                        IconClass = IconClass,
                        IconColor = (IsImpediment ?? false) ? "darkred" : "auto",
                        HtmlBody = HtmlBody,
                        TextBody = TextBody
                    });
                }

                await PendingWorkLog.FromCommentAsync(connection, this, user as UserProfile);
                await ParseMentionsAsync(connection, this, user as UserProfile);
            }
        }

        /// <summary>
        /// Queues notifications to people from comment text based on @ symbols
        /// </summary>
        private async Task ParseMentionsAsync(IDbConnection connection, Comment comment, UserProfile userProfile)
        {
            // for now, can do mentions only on work item comments because EventLog.WorkItemId is required
            if (comment.ObjectType != ObjectType.WorkItem) return;
            
            var names = Regex.Matches(comment.TextBody, "@([a-zA-Z][a-zA-Z0-9_]*)").OfType<Match>();

            var sender = await connection.FindWhereAsync<OrganizationUser>(new { comment.OrganizationId, userProfile.UserId });
            string senderName = sender.DisplayName ?? userProfile.UserName;

            var mentionedUsers = new HashSet<OrganizationUser>();
            foreach (var name in names)
            {
                var users = await new OrgUserByName() { OrgId = comment.OrganizationId, SearchName = name.Value.Substring(1) }.ExecuteAsync(connection);
                if (users.Any())
                {
                    var mentionedUser = users.First();
                    mentionedUsers.Add(mentionedUser);
                    await ReplaceMentionNameAsync(connection, comment, name.Value, mentionedUser);
                    await AddMentionEventInnerAsync(connection, comment, senderName, mentionedUser);
                }
            }

            var workItem = await connection.FindAsync<WorkItem>(comment.ObjectId);
            if (workItem.DeveloperUserId.HasValue && workItem.DeveloperUserId != sender.UserId)
            {
                var devUser = await connection.FindWhereAsync<OrganizationUser>(new { workItem.OrganizationId, UserId = workItem.DeveloperUserId.Value });
                if (!mentionedUsers.Contains(devUser) && devUser.AllowNotification()) await AddMentionEventInnerAsync(connection, comment, senderName, devUser);                
            }

            if (workItem.BusinessUserId.HasValue && workItem.BusinessUserId != sender.UserId)
            {
                var bizUser = await connection.FindWhereAsync<OrganizationUser>(new { workItem.OrganizationId, UserId = workItem.BusinessUserId.Value });
                if (!mentionedUsers.Contains(bizUser) && bizUser.AllowNotification()) await AddMentionEventInnerAsync(connection, comment, senderName, bizUser);
            }
        }

        private async Task AddMentionEventInnerAsync(IDbConnection connection, Comment comment, string senderName, OrganizationUser mentionedUser)
        {
            string mentionName = mentionedUser.DisplayName ?? mentionedUser.Email;            
            int eventLogId = await CreateEventLogFromMentionAsync(connection, comment, senderName, mentionName);
            await Notification.CreateFromMentionAsync(connection, eventLogId, comment, senderName, mentionedUser);
        }

        private async Task<int> CreateEventLogFromMentionAsync(IDbConnection connection, Comment comment, string senderName, string mentionName)
        {
            var workItem = await connection.FindAsync<WorkItem>(comment.ObjectId);

            return await EventLog.WriteAsync(connection, new EventLog()
            {
                DateCreated = comment.DateCreated,                
                OrganizationId = comment.OrganizationId,
                TeamId = workItem.TeamId,
                ApplicationId = workItem.ApplicationId,
                WorkItemId = workItem.Id,
                EventId = SystemEvent.UserMentioned,
                IconClass = "fas fa-at",
                IconColor = "auto",
                HtmlBody = $"<strong>{senderName}</strong> mentioned <strong>{mentionName}</strong> in a comment",
                TextBody = $"{senderName} mentioned {mentionName} in a comment",
                SourceId = comment.Id,
                SourceTable = nameof(Comment)
            });
        }

        private async Task ReplaceMentionNameAsync(IDbConnection connection, Comment comment, string mentionName, OrganizationUser orgUser)
        {
            string result = comment.HtmlBody;
            result = result.Replace(mentionName, $"<a href=\"mailto:{orgUser.Email}\">{orgUser.DisplayName ?? orgUser.UserName}</a>");
            comment.HtmlBody = result;
            comment.TextBody = new Converter().Convert(comment.HtmlBody);
            await connection.SaveAsync(comment);
        }

        public async Task<int> GetOrgIdAsync(IDbConnection connection)
        {
            return await Task.FromResult(OrganizationId);
        }
    }
}