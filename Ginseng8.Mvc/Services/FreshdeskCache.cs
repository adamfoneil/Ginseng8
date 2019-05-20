using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Models.Freshdesk.Dto;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
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
    }
}