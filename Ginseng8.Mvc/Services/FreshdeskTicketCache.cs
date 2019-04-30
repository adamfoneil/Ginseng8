using Ginseng.Integration.Services;
using Ginseng.Models;
using Ginseng.Models.Freshdesk;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Postulate.SqlServer.IntKey;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Services
{
	public class FreshdeskTicketCache : IntegrationCache<Ticket>
	{		
		private readonly DataAccess _data;

		public FreshdeskTicketCache(IConfiguration config) : base(config, "Tickets")
		{			
			_data = new DataAccess(config);
		}

		public override TimeSpan CallInterval => TimeSpan.FromMinutes(5);		

		protected override async Task<IEnumerable<Ticket>> CallApiAsync(string orgName)
		{
			using (var cn = _data.GetConnection())
			{
				var org = await cn.FindWhereAsync<Organization>(new { name = orgName });
				var client = new FreshdeskClient(org.FreshdeskUrl, org.FreshdeskApiKey);
				return client.ListTickets();
			}
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