using Ginseng.Models.Enums.Freshdesk;
using Newtonsoft.Json;
using System;

namespace Ginseng.Mvc.Models.Freshdesk.Dto
{
    public class Ticket
    {   /*
		[JsonProperty("cc_emails")]
		public string[] CcEmails { get; set; }

		[JsonProperty("fwd_emails")]
		public string[] FwdEmails { get; set; }

		[JsonProperty("reply_cc_emails")]
		public string[] ReplyCcEmails { get; set; }

		[JsonProperty("ticket_cc_emails")]
		public string[] TicketCcEmails { get; set; }

		[JsonProperty("fr_escalated")]
		public bool FrEscalated { get; set; }

		[JsonProperty("spam")]
		public bool Spam { get; set; }

		[JsonProperty("email_config_id")]
		public long? EmailConfigId { get; set; }
        */

		[JsonProperty("group_id")]
		public long? GroupId { get; set; }

        /*
		[JsonProperty("priority")]
		public int Priority { get; set; }

		[JsonProperty("requester_id")]
		public long RequesterId { get; set; }

		[JsonProperty("responder_id")]
		public long? ResponderId { get; set; }

		[JsonProperty("source")]
		public long Source { get; set; }

		[JsonProperty("company_id")]
		public long? CompanyId { get; set; }
        */

        [JsonProperty("status")]
        public TicketStatus Status { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        /*
		[JsonProperty("association_type")]
		public string AssociationType { get; set; }

		[JsonProperty("to_emails")]
		public string[] ToEmails { get; set; }

		[JsonProperty("product_id")]
		public long ProductId { get; set; }
        */

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
        
        /*		
		[JsonProperty("due_by")]
		public DateTimeOffset DueBy { get; set; }

		[JsonProperty("fr_due_by")]
		public DateTimeOffset FrDueBy { get; set; }

		[JsonProperty("is_escalated")]
		public bool IsEscalated { get; set; }
        */

        [JsonProperty("custom_fields")]
        public CustomFields CustomFields { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /*
         [JsonProperty("updated_at")]
         public DateTimeOffset UpdatedAt { get; set; }

         [JsonProperty("associated_tickets_count")]
         public int? AssociatedTicketsCount { get; set; }

         [JsonProperty("tags")]
         public string[] Tags { get; set; }
         */

        public static Ticket FromJson(string json) => JsonConvert.DeserializeObject<Ticket>(json);
    }
}