using Ginseng.Models.Enums.Freshdesk;

namespace Ginseng.Mvc.Models.Freshdesk
{
    public class TicketInfo
    {
        public long Id { get; set; }
        public string Subject { get; set; }
        public TicketStatus Status { get; set; }
        public TicketType Type { get; set; }
        public string Url { get; set; }
    }
}