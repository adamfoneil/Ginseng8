using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Ginseng.Models;
using Ginseng.Mvc.Models.Freshdesk;
using Microsoft.AspNetCore.Mvc;

namespace Ginseng.Mvc.Interfaces
{
    /// <summary>
    /// Freshdesk integration service
    /// </summary>
    public interface IFreshdeskService
    {
        /// <summary>
        /// Validates webhook's api key
        /// </summary>
        /// <param name="key">Webhook's api key</param>
        /// <returns><c>True</c> if valid, otherwise <c>False</c></returns>
        bool ValidateWebhookApiKey(string key);

        /// <summary>
        /// Stores (uploads to Azure Blobs) received webhook DTO payload (before mapping) when <c>StoreWebhookPayload</c> options is enabled
        /// </summary>
        /// <remarks>
        /// Used of debug purposes
        /// </remarks>
        /// <param name="request">Webhook DTO request stream</param>
        /// <returns>Task</returns>
        Task StoreWebhookPayloadAsync(Stream request);

        /// <summary>
        /// Stores request and result content for troubleshooting failed webhook calls
        /// </summary>
        Task StoreWebhookPayloadAsync(Stream request, IActionResult result);

        /// <summary>
        /// On webhook received event
        /// </summary>
        /// <param name="webhook">Webhook model</param>
        /// <returns>Task</returns>
        Task OnWebhookAsync(Webhook webhook);

        /// <summary>
        /// On a work item updated event
        /// </summary>
        /// <param name="workItem">Work item</param>
        /// <returns>Task</returns>
        Task OnWorkItemUpdatedAsync(WorkItem workItem);

        /// <summary>
        /// On a work item deleted event
        /// </summary>
        /// <param name="workItem">Work item</param>
        /// <returns>Task</returns>
        Task OnWorkItemDeletedAsync(WorkItem workItem);

        /// <summary>
        /// Assigns an existed work item to Freshdesk's tickets by their ids
        /// </summary>
        /// <param name="workItem">Work item to assign Freshdesk tickets</param>
        /// <param name="ticketIds">Freshdesk's ticket ids to assign the work item</param>
        /// <returns>Task</returns>
        Task AssignWorkItemToTicketsAsync(WorkItem workItem, IEnumerable<long> ticketIds);
    }
}
