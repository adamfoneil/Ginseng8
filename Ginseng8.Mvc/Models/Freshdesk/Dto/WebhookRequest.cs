using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Ginseng.Mvc.Models.Freshdesk.Dto
{
    public class WebhookRequest
    {
        [Required]
        [JsonProperty("event")]
        public string Event { get; set; }

        [Required]
        [JsonProperty("ticket")]
        public TicketInfo Ticket { get; set; }

        public class TicketInfo
        {
            [Required]
            [JsonProperty("id")]
            public long Id { get; set; }

            [Required]
            [JsonProperty("subject")]
            public string Subject { get; set; }

            [Required]
            [JsonProperty("status")]
            public string Status { get; set; }
           
            [JsonProperty("type")]
            public string Type { get; set; }

            [Required]
            [JsonProperty("url")]
            public string Url { get; set; }
        }
    }
}