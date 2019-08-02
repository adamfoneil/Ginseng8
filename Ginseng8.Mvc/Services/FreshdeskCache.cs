using Ginseng.Models;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Mapping;
using Ginseng.Mvc.Models.Freshdesk.Dto;
using Microsoft.Extensions.Configuration;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Services
{
    public class FreshdeskCache
    {
        public FreshdeskCache(IConfiguration config, IFreshdeskClientFactory clientFactory)
        {
            TicketCache = new FreshdeskTicketCache(config, clientFactory);
            GroupCache = new FreshdeskGroupCache(config, clientFactory);
            CompanyCache = new FreshdeskCompanyCache(config, clientFactory);
            ContactCache = new FreshdeskContactCache(config, clientFactory);
        }

        public string GetContactName(long requesterId)
        {
            return (ContactDictionary.ContainsKey(requesterId)) ? ContactDictionary[requesterId].Name : $"requester id {requesterId}";
        }

        public string GetCompanyName(long companyId)
        {
            return (CompanyDictionary.ContainsKey(companyId)) ? CompanyDictionary[companyId].Name : $"company id {companyId}";
        }

        public FreshdeskTicketCache TicketCache { get; }
        public FreshdeskGroupCache GroupCache { get; }
        public FreshdeskCompanyCache CompanyCache { get; }
        public FreshdeskContactCache ContactCache { get; }

        public IEnumerable<Ticket> Tickets { get; private set; }
        public IEnumerable<Contact> Contacts { get; private set; }
        public IEnumerable<Company> Companies { get; private set; }
        public IEnumerable<Group> Groups { get; private set; }

        public Dictionary<long, Group> GroupDictionary { get; set; }
        public Dictionary<long, Contact> ContactDictionary { get; set; }
        public Dictionary<long, Company> CompanyDictionary { get; set; }

        public async Task InitializeAsync(string orgName)
        {
            Tickets = await TicketCache.QueryAsync(orgName);

            Contacts = await ContactCache.QueryAsync(orgName);
            ContactDictionary = Contacts.ToDictionary(row => row.Id);
            
            Companies = await CompanyCache.QueryAsync(orgName);
            CompanyDictionary = Companies.ToDictionary(row => row.Id);

            Groups = await GroupCache.QueryAsync(orgName);
            GroupDictionary = Groups.ToDictionary(row => row.Id);
        }

        public async Task LinkWorkItemToTicketAsync(
            SqlConnection cn, IFreshdeskClient client, int orgId, int workItemNumber, Ticket ticket, UserProfile currentUser)
        {
            var wit = new WorkItemTicket()
            {
                TicketId = ticket.Id,
                WorkItemNumber = workItemNumber,
                OrganizationId = orgId,
                TicketStatus = ticket.Status,
                TicketType = WebhookRequestToWebhookConverter.TicketTypeFromString(ticket.Type),
                CompanyId = ticket.CompanyId,
                CompanyName = GetCompanyName(ticket.CompanyId ?? 0),
                ContactId = ticket.RequesterId,
                ContactName = GetContactName(ticket.RequesterId),
                Subject = ticket.Subject
            };

            await client.UpdateTicketWorkItemAsync(ticket.Id, workItemNumber.ToString());
            await cn.SaveAsync(wit, currentUser);
        }
    }
}