using Ginseng.Mvc.Models.Freshdesk.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ginseng.Mvc.Interfaces;
using System.Linq;

namespace Ginseng.Mvc.Services
{
	public class FreshdeskTicketCache : IntegrationCache<Ticket>
	{		
		private readonly DataAccess _data;
        private readonly IFreshdeskClientFactory _clientFactory;

		public FreshdeskTicketCache(
            IConfiguration config,
            IFreshdeskClientFactory clientFactory) 
            : base(config, "Tickets")
		{			
			_data = new DataAccess(config);
            _clientFactory = clientFactory;
        }

		public override TimeSpan CallInterval => TimeSpan.FromMinutes(5);		

		protected override async Task<IEnumerable<Ticket>> ApiQueryAll(string orgName)
        {
            var client = await _clientFactory.CreateClientForOrganizationAsync(orgName);
            var tickets = await client.ListTicketsAsync();
            return tickets.OrderBy(row => row.CreatedAt);
		}

		protected override async Task<Ticket> ConvertFromBlobAsync(CloudBlockBlob blob)
		{
			string json = await blob.DownloadTextAsync();
			return JsonConvert.DeserializeObject<Ticket>(json);
		}

		protected override string GetBlobName(Ticket @object)
		{
			return @object.Id.ToString() + ".json";
		}
    }
}