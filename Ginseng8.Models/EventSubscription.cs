using Ginseng.Models.Conventions;
using Ginseng.Models.Interfaces;
using Postulate.Base.Attributes;

namespace Ginseng.Models
{
    public class EventSubscription : BaseTable, INotifyOptions
    {
        [References(typeof(Event))]
        [PrimaryKey]
        public int EventId { get; set; }

        [References(typeof(Organization))]
        [PrimaryKey]
        public int OrganizationId { get; set; }

        [References(typeof(Application))]
        [PrimaryKey]
        public int ApplicationId { get; set; }

        [References(typeof(UserProfile))]
        [PrimaryKey]
        public int UserId { get; set; }

        /// <summary>
        /// Events shown on Dashboard/Feed
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Send an email for this event
        /// </summary>
        public bool SendEmail { get; set; }

        /// <summary>
        /// Send a text for this event
        /// </summary>
        public bool SendText { get; set; }

        /// <summary>
        /// Show notification within the site
        /// </summary>
        public bool InApp { get; set; }

        public string TableName => nameof(EventSubscription);

        public bool AllowNotification()
        {
            return NotifyOptionsImplementation.AllowNotification(this);
        }
    }
}