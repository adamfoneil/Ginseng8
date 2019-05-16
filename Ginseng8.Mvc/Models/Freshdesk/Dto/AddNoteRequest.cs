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

        [JsonProperty("body")]
        public string Body => $"{_comment.HtmlBody}<br/>from {_userName}";

        [JsonProperty("private")]
        public bool Private { get; set; } = true;

        public string ToJson() => JsonConvert.SerializeObject(this);
    }
}