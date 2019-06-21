using Newtonsoft.Json;

namespace Ginseng.Mvc.Models.Freshdesk.Dto
{
    /// <summary>
    /// see https://developers.freshdesk.com/api/#add_note_to_a_ticket
    /// </summary>
    public class AddNoteRequest
    {
        private readonly Ginseng.Models.Comment _comment;
        private readonly string _userName;

        public AddNoteRequest(Ginseng.Models.Comment comment, string userName)
        {
            _comment = comment;
            _userName = userName;
        }

        /// <summary>
        /// This came from inspecting sample note payload when creating online
        /// </summary>
        [JsonIgnore]
        public string Style { get; set; } = "font-family:-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, Helvetica Neue, Arial, sans-serif; font-size:14px;";

        [JsonProperty("body")]
        public string Body => $"<div style=\"{Style}\">{_comment.HtmlBody}<br/>from {_userName}</div>";

        [JsonProperty("private")]
        public bool Private { get; set; } = true;

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}