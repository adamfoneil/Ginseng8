namespace Ginseng.Mvc.Models.Freshdesk
{
    /// <summary>
    /// Freshdesk service configuration options
    /// </summary>
    public class FreshdeskServiceOptions
    {
        /// <summary>
        /// Determines that received webhook payload has to be stored (uploaded to Azure blobs)
        /// </summary>
        /// <remarks>
        /// Used for debug purposes
        /// </remarks>
        public bool StoreWebhookPayload { get; set; }

        /// <summary>
        /// Webhook's api key to secure Webhook endpoints from unauthorized use
        /// </summary>
        public string WebhookKey { get; set; }
    }
}
