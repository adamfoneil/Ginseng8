using Newtonsoft.Json;
using System;

namespace Ginseng.Mvc.Models.Freshdesk.Dto
{
    public partial class Contact
    {
        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("address")]
        public object Address { get; set; }

        [JsonProperty("company_id")]
        public long? CompanyId { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("job_title")]
        public string JobTitle { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("time_zone")]
        public string TimeZone { get; set; }

        [JsonProperty("twitter_id")]
        public string TwitterId { get; set; }

        [JsonProperty("custom_fields")]
        public ContactCustomFields CustomFields { get; set; }

        [JsonProperty("facebook_id")]
        public object FacebookId { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public partial class ContactCustomFields
    {
        [JsonProperty("vip")]
        public object Vip { get; set; }

        [JsonProperty("aeriehub_role")]
        public object AeriehubRole { get; set; }
    }
}