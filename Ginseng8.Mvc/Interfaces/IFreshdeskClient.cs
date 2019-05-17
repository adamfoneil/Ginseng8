using System.Collections.Generic;
using System.Threading.Tasks;
using Ginseng.Mvc.Models.Freshdesk.Dto;

namespace Ginseng.Mvc.Interfaces
{
    public interface IFreshdeskClient
    {
        /// <summary>
        /// Retrieves all groups
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Group>> ListGroupsAsync();

        Task<IEnumerable<Contact>> ListContactsAsync();

        Task<Contact> GetContactAsync(long id);

        /// <summary>
        /// Retrieves all tickets
        /// </summary>
        /// <returns>Tickets list</returns>
        Task<IEnumerable<Ticket>> ListTicketsAsync();

        /// <summary>
        /// Retrieves a ticket by its id
        /// </summary>
        /// <param name="id">Ticket id to retrieve</param>
        /// <returns>Ticket</returns>
        Task<Ticket> GetTicketAsync(long id);

        /// <summary>
        /// Updates Freshdesk ticket' field that projected Ginseng8 work item
        /// </summary>
        /// <param name="id">Ticket id to update</param>
        /// <param name="value">Ginseng work item field raw value</param>
        /// <returns>Task</returns>
        Task UpdateTicketWorkItemAsync(long id, string value);

        Task AddNoteAsync(long id, Ginseng.Models.Comment comment, string userName);
    }
}
