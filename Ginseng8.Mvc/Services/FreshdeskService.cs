using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Ginseng.Models;
using Ginseng.Mvc.Enums.Freshdesk;
using Ginseng.Mvc.Exceptions;
using Ginseng.Mvc.Extensions;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Models.Freshdesk;
using Ginseng.Mvc.Queries;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Postulate.SqlServer.IntKey;

namespace Ginseng.Mvc.Services
{
    /// <summary>
    /// Freshdesk integration service
    /// </summary>
    public class FreshdeskService : IFreshdeskService
    {
        /// <summary>
        /// Service options
        /// </summary>
        private readonly FreshdeskServiceOptions _options;

        /// <summary>
        /// Data access service
        /// </summary>
        private readonly DataAccess _data;

        /// <summary>
        /// Azure Blobs service
        /// </summary>
        private readonly BlobStorage _blobStorage;

        /// <summary>
        /// Freshdesk client factory
        /// </summary>
        private readonly IFreshdeskClientFactory _clientFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="options">Service options</param>
        /// <param name="clientFactory">Freshdesk client factory</param>
        public FreshdeskService(
            IConfiguration config,
            IOptions<FreshdeskServiceOptions> options,
            IFreshdeskClientFactory clientFactory)
        {
            _data = new DataAccess(config);
            _blobStorage = new BlobStorage(config);
            _options = options.Value;
            _clientFactory = clientFactory;
        }

        /// <inheritdoc />
        public bool ValidateWebhookApiKey(string key)
            => _options.WebhookKey == key;

        /// <inheritdoc />
        public async Task OnWebhookAsync(Webhook webhook)
        {
            if (webhook == null) throw new ArgumentNullException(nameof(webhook));

            switch (webhook.Event)
            {
                case WebhookEvent.TicketDeleted:
                case WebhookEvent.TicketUpdated:
                    await OnTicketChangedAsync(webhook.Ticket, webhook.Received);
                    return;
            }
        }

        /// <inheritdoc />
        public async Task AssignWorkItemToTicketsAsync(WorkItem workItem, IEnumerable<long> ticketIds)
        {
            if (!ticketIds.Any()) return;

            using (var cn = _data.GetConnection())
            {
                var organization = await GetOrganizationAsync(cn, workItem.OrganizationId);

                // create related work item tickets
                var workItemTickets = new List<WorkItemTicket>();
                foreach (var ticketId in ticketIds)
                {
                    var workItemTicket = new WorkItemTicket()
                    {
                        TicketId = ticketId,
                        OrganizationId = workItem.OrganizationId,
                        TicketStatus = 0,
                        WorkItemNumber = workItem.Number,
                        CreatedBy = "system",
                        DateCreated = DateTime.UtcNow,
                    };

                    await cn.InsertAsync(workItemTicket);
                    workItemTickets.Add(workItemTicket);
                }

                await ReadTicketsAsync(organization, workItemTickets);
                await UpdateTicketsAsync(organization, workItem, workItemTickets);
            }
        }

        /// <inheritdoc />
        public async Task OnWorkItemUpdatedAsync(WorkItem workItem)
        {
            using (var cn = _data.GetConnection())
            {
                var organization = await GetOrganizationAsync(cn, workItem.OrganizationId);

                // retrieve all work item tickets related to the work item
                var workItemTickets = await new WorkItemTickets()
                {
                    WorkItemNumber = workItem.Number,
                    OrganizationId = workItem.OrganizationId
                }.ExecuteAsync(cn);

                if (!workItemTickets.Any()) return;
                
                await UpdateTicketsAsync(organization, workItem, workItemTickets);
            }
        }

        /// <inheritdoc />
        public async Task OnWorkItemDeletedAsync(WorkItem workItem)
        {
            using (var cn = _data.GetConnection())
            {
                var organization = await GetOrganizationAsync(cn, workItem.OrganizationId);

                // retrieve all work item tickets related to the work item
                var workItemTickets = await new WorkItemTickets()
                {
                    WorkItemNumber = workItem.Number,
                    OrganizationId = workItem.OrganizationId
                }.ExecuteAsync(cn);

                if (!workItemTickets.Any()) return;

                await ClearTicketsAsync(organization, workItem, workItemTickets);

                /*
                // delete all related work item tickets from database
                foreach (var workItemTicket in workItemTickets)
                {
                    await cn.DeleteAsync<WorkItemTickets>(workItemTicket.Id);
                }
                */
            }
        }

        /// <inheritdoc />
        public async Task StoreWebhookPayloadAsync(Stream stream)
        {
            if (!_options.StoreWebhookPayload) return;

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            string payload;
            using (var reader = new StreamReader(stream))
            {
                payload = await reader.ReadToEndAsync();
            }

            var container = await _blobStorage.GetContainerAsync("freshdesk-webhooks");
            var blob = container.GetBlockBlobReference(Guid.NewGuid().ToString());

            await blob.UploadTextAsync(payload);
        }

        /// <summary>
        /// On ticket changed notification received through a Webhook
        /// </summary>
        /// <param name="ticket">Ticket information</param>
        /// <param name="webhookReceived">Webhook's receive time</param>
        /// <returns>Task</returns>
        private async Task OnTicketChangedAsync(TicketInfo ticket, DateTimeOffset webhookReceived)
        {
            if (ticket == null) throw new ArgumentNullException(nameof(ticket));

            using (var cn = _data.GetConnection())
            {
                // find the organization related to Freshdesk's ticket (by url)
                var organization = await GetOrganizationAsync(cn, ticket.Url);

                // find work item tickets associated with the Freshdesk's ticket
                var workItemTickets = await new WorkItemTickets()
                {
                    TicketId = ticket.Id,
                    OrganizationId = organization.Id
                }.ExecuteAsync(cn);

                if (!workItemTickets.Any()) return;

                // update statuses if changed
                foreach (var workItemTicket in workItemTickets)
                {
                    if (workItemTicket.TicketStatus == (int) ticket.Status) continue;

                    workItemTicket.TicketStatus = (int) ticket.Status;
                    workItemTicket.TicketStatusDateModified = webhookReceived.UtcDateTime;

                    await cn.UpdateAsync(workItemTicket);
                }
            }
        }

        /// <summary>
        /// Finds organization related to the same Freshdesk host
        /// </summary>
        /// <param name="cn">SQL connection</param>
        /// <param name="url">Absolute url of Freshdesk's ticket or another Freshdesk's entity</param>
        /// <exception cref="NotFoundException">Thrown when the related organization does not exist in database</exception>
        /// <returns>Organization related to the url</returns>
        private async Task<Organization> GetOrganizationAsync(SqlConnection cn, string url)
        {
            if (url.IsNullOrEmpty()) throw new ArgumentNullException(nameof(url));
         
            // note: in this case every organization must have own unique Freshdesk URL,
            // otherwise another algorithm should be used in order to determine ticket's organization
            var organization = await new OrganizationByFreshdeskHost() {Url = url }.ExecuteSingleAsync(cn);
            if (organization == null) throw new NotFoundException("Organization was not found related to the ticket");

            return organization;
        }

        /// <summary>
        /// Finds organization by its id
        /// </summary>
        /// <param name="cn">SQL connection</param>
        /// <param name="id">Organization id to find</param>
        /// <exception cref="NotFoundException">Thrown when the related organization does not exist in database</exception>
        /// <returns>Organization</returns>
        private async Task<Organization> GetOrganizationAsync(SqlConnection cn, int id)
        {
            var organization = await cn.FindAsync<Organization>(id);
            if (organization == null) throw new NotFoundException("Organization was not found related to the work item");

            return organization;
        }

        /// <summary>
        /// Reads Freshdesk tickets related to work item tickets and updates work item ticket statuses accordingly
        /// </summary>
        /// <remarks>
        /// Freshdesk -> Ginseng8 synchronization
        /// </remarks>
        /// <param name="organization">Organization</param>
        /// <param name="workItemTickets">Work item tickets to update their statuses</param>
        /// <returns>Task</returns>
        private async Task ReadTicketsAsync(Organization organization, IEnumerable<WorkItemTicket> workItemTickets)
        {
            using (var cn = _data.GetConnection())
            {
                var utcNow = DateTime.UtcNow;
                var client = _clientFactory.CreateClientForOrganization(organization);
                foreach (var workItemTicket in workItemTickets)
                {
                    try
                    {
                        var ticket = await client.GetTicketAsync(workItemTicket.TicketId);
                        if (ticket == null) continue;

                        workItemTicket.TicketStatus = ticket.Status;
                        workItemTicket.TicketStatusDateModified = utcNow;

                        await cn.UpdateAsync(workItemTicket);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

        /// <summary>
        /// Updates all Freshdesk tickets related to work item tickets
        /// </summary>
        /// <remarks>
        /// Ginseng8 -> Freshdesk synchronization
        /// </remarks>
        /// <param name="organization">Organization</param>
        /// <param name="workItem">Work item</param>
        /// <param name="workItemTickets">Work item tickets related to the work item</param>
        /// <returns>Task</returns>
        private async Task UpdateTicketsAsync(Organization organization, WorkItem workItem, IEnumerable<WorkItemTicket> workItemTickets)
        {
            using (var cn = _data.GetConnection())
            {
                var client = _clientFactory.CreateClientForOrganization(organization);
                foreach (var workItemTicket in workItemTickets)
                {
                    try
                    {
                        var status = await GetWorkItemStatusAsync(cn, workItem);
                        var fieldValue = $"{workItem.Number} {status}";
                        await client.UpdateTicketWorkItemAsync(workItemTicket.TicketId, fieldValue);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

        /// <summary>
        /// Clears Ginseng8's related information in all Freshdesk tickets related to work item tickets
        /// </summary>
        /// <remarks>
        /// Ginseng8 -> Freshdesk synchronization
        /// </remarks>
        /// <param name="organization">Organization</param>
        /// <param name="workItem">Work item</param>
        /// <param name="workItemTickets">Work item tickets related to the work item</param>
        /// <returns>Task</returns>
        private async Task ClearTicketsAsync(Organization organization, WorkItem workItem, IEnumerable<WorkItemTicket> workItemTickets)
        {
            using (var cn = _data.GetConnection())
            {
                var client = _clientFactory.CreateClientForOrganization(organization);
                foreach (var workItemTicket in workItemTickets)
                {
                    try
                    {
                        await client.UpdateTicketWorkItemAsync(workItemTicket.TicketId, "");
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

        /// <summary>
        /// Returns work item status text
        /// </summary>
        /// <param name="cn">SQL connection</param>
        /// <param name="workItem">Work item</param>
        /// <returns>Work item staus text</returns>
        private async Task<string> GetWorkItemStatusAsync(SqlConnection cn, WorkItem workItem)
        {
            if (!workItem.CloseReasonId.HasValue) return "Open";

            var closeReason = await cn.QuerySingleAsync<string>(
                "SELECT [Name] FROM [app].[CloseReason] WHERE [Id] = @id",
                new {id = workItem.CloseReasonId});

            if (closeReason.IsNullOrEmpty()) return "Closed";

            return closeReason;
        }
    }
}
