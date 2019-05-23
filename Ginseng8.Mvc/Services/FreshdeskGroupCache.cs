using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Models.Freshdesk.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Services
{
    public class FreshdeskGroupCache : IntegrationCache<Group>
    {
        private readonly IFreshdeskClientFactory _clientFactory;

        public FreshdeskGroupCache(
            IConfiguration config,
            IFreshdeskClientFactory factory) : base(config, "Groups")
        {
            _clientFactory = factory;
        }

        public override TimeSpan CallInterval => TimeSpan.FromHours(1);

        protected override async Task<IEnumerable<Group>> ApiQueryAll(string orgName)
        {
            var client = await _clientFactory.CreateClientForOrganizationAsync(orgName);
            var groups = await client.ListGroupsAsync();
            return groups;
        }

        protected override async Task<Group> ConvertFromBlobAsync(CloudBlockBlob blob)
        {
            string json = await blob.DownloadTextAsync();
            return JsonConvert.DeserializeObject<Group>(json);
        }

        protected override string GetBlobName(Group @object)
        {
            return @object.Name;
        }
    }
}