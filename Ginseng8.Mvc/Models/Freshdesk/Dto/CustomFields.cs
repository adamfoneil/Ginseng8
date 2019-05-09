using Newtonsoft.Json;

namespace Ginseng.Mvc.Models.Freshdesk.Dto
{
    public class CustomFields
    {
        [JsonProperty("cf_ginsengworkitem")]
        public string GinsengWorkItem { get; set; }
    }
}
