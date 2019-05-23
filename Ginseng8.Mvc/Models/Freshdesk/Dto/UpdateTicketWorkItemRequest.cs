using Newtonsoft.Json;

namespace Ginseng.Mvc.Models.Freshdesk.Dto
{
    public class UpdateTicketWorkItemRequest
    {
        public UpdateTicketWorkItemRequest(string ginsengWorkItem)
        {
            CustomFields = new CustomFields()
            {
                GinsengWorkItem = ginsengWorkItem
            };
        }

        [JsonProperty("custom_fields")]
        public CustomFields CustomFields { get; set; }

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}
