using Newtonsoft.Json;

namespace Ginseng.Mvc.Models.Freshdesk.Dto
{
    public class CustomFields
    {
        [JsonProperty("cf_work_item")]
        public string GinsengWorkItem { get; set; }
    }
}
