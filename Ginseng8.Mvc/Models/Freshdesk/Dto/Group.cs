using Newtonsoft.Json;
using System;

namespace Ginseng.Mvc.Models.Freshdesk.Dto
{
    public class Group
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("escalate_to")]
        public object EscalateTo { get; set; }

        [JsonProperty("unassigned_for")]
        public object UnassignedFor { get; set; }

        [JsonProperty("business_hour_id")]
        public object BusinessHourId { get; set; }

        [JsonProperty("group_type")]
        public string GroupType { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}