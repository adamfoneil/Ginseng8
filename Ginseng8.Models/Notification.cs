using Ginseng.Models.Queries;
using Postulate.Base;
using Postulate.Base.Attributes;
using Postulate.Base.Interfaces;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ginseng.Models
{
    public enum DeliveryMethod
    {
        Email = 1,
        Text = 2,
        App = 3
    }

    /// <summary>
    /// Holds pending and delivered notifications. This is queried through a call from cron-job.org
    /// every 10 minutes or whatever that sends notifications in batches of 50 or some such.
    /// </summary>
    [Identity(nameof(Id))]
    public class Notification : IFindRelated<int>
    {
        public int Id { get; set; }

        [References(typeof(EventLog))]
        public int EventLogId { get; set; }

        /// <summary>
        /// User's local time
        /// </summary>
        public DateTime DateCreated { get; set; }

        public DeliveryMethod Method { get; set; }

        [MaxLength(255)]
        [Required]
        public string SendTo { get; set; }

        [Required]
        public string Content { get; set; }

        /// <summary>
        /// EventSubscription.Id or ActivitySubscription.Id (used for unsubscribe link)
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>
        /// EventSubscription or ActivitySubscription (needed for unsubscribe link)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SourceTable { get; set; }

        /// <summary>
        /// Indicates when message was delivered. If null, then it's still pending
        /// </summary>
        public DateTime? DateDelivered { get; set; }

        public EventLog EventLog { get; set; }

        public static async Task CreateFromEventSubscriptions(IDbConnection connection, int eventLogId)
        {
            await new InsertEventSubscriptionEmailNotifications() { Id = eventLogId }.ExecuteAsync(connection);
            await new InsertEventSubscriptionTextNotifications() { Id = eventLogId }.ExecuteAsync(connection);
            await new InsertEventSubscriptionAppNotifications() { Id = eventLogId }.ExecuteAsync(connection);            
        }

        internal static async Task CreateFromLabelSubscriptions(IDbConnection connection, int eventLogId)
        {
            await new InsertLabelSubscriptionEmailNotifications() { Id = eventLogId }.ExecuteAsync(connection);
            await new InsertLabelSubscriptionAppNotifications() { Id = eventLogId }.ExecuteAsync(connection);
            // todo: text notifications
        }

        public static async Task CreateFromActivitySubscriptions(IDbConnection connection, int eventLogId)
        {
            await new InsertActivitySubscriptionEmailNotifications() { Id = eventLogId }.ExecuteAsync(connection);
            await new InsertActivitySubscriptionTextNotifications() { Id = eventLogId }.ExecuteAsync(connection);
            await new InsertActivitySubscriptionAppNotifications() { Id = eventLogId }.ExecuteAsync(connection);            
        }

        public static async Task CreateFromWorkItemAssignment(IDbConnection connection, int eventLogId)
        {
            await new InsertAssignmentDevEmailNotification() { Id = eventLogId }.ExecuteAsync(connection);
            await new InsertAssignmentBizEmailNotification() { Id = eventLogId }.ExecuteAsync(connection);
            
            await new InsertAssignedDevAppNotification() { Id = eventLogId }.ExecuteAsync(connection);
            await new InsertAssignmentBizAppNotification() { Id = eventLogId }.ExecuteAsync(connection);

            // todo: text notifications
        }

        internal static async Task CreateFromMentionAsync(IDbConnection connection, int eventLogId, Comment comment, string senderName, OrganizationUser mentionUser)
        {
            if (mentionUser.SendEmail)
            {
                await CreateFromCommentAsync(connection, eventLogId, comment, DeliveryMethod.Email, senderName, mentionUser, (ou) => ou.Email, (c) => c.HtmlBody);
            }

            if (mentionUser.SendText)
            {
                await CreateFromCommentAsync(connection, eventLogId, comment, DeliveryMethod.Text, senderName, mentionUser, (ou) => ou.PhoneNumber, (c) => c.TextBody);
            }

            // todo: app notification
        }

        private static async Task CreateFromCommentAsync(
            IDbConnection connection, int eventLogId, Comment comment, DeliveryMethod method, string senderName, OrganizationUser mentionUser,
            Func<OrganizationUser, string> addressGetter, Func<Comment, string> contentGetter)
        {
            string sendTo = addressGetter.Invoke(mentionUser);
            if (string.IsNullOrEmpty(sendTo)) return;

            await connection.SaveAsync(new Notification()
            {
                EventLogId = eventLogId,
                SendTo = sendTo,
                DateCreated = comment.DateCreated,
                Method = method,
                Content = $"{senderName} writes: {contentGetter.Invoke(comment)}",
                SourceId = comment.Id,
                SourceTable = nameof(Comment)
            });
        }

        public void FindRelated(IDbConnection connection, CommandProvider<int> commandProvider, IUser user = null, IEnumerable<Claim> claims = null)
        {
            EventLog = commandProvider.Find<EventLog>(connection, EventLogId);
        }

        public async Task FindRelatedAsync(IDbConnection connection, CommandProvider<int> commandProvider, IUser user = null, IEnumerable<Claim> claims = null)
        {
            EventLog = await commandProvider.FindAsync<EventLog>(connection, EventLogId);
        }
    }
}