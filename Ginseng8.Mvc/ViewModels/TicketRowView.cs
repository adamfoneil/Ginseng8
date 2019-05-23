using Ginseng.Mvc.Models.Freshdesk.Dto;
using Ginseng.Mvc.Pages.Tickets;

namespace Ginseng.Mvc.ViewModels
{
    public class TicketRowView
    {
        public string FreshdeskUrl { get; set; }
        public int RowIndex { get; set; }
        public TicketPageModel PageModel { get; set; }
        public Ticket Ticket { get; set; }
    }
}