using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ginseng.Mvc.Extensions;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Models.Freshdesk.Dto;

namespace Ginseng.Integration.Services
{
	public class FreshdeskClient : IFreshdeskClient
    {
		private readonly string _hostUrl;
		private readonly string _apiKey;
		private readonly string _endpointPrefix;

		public FreshdeskClient(string hostUrl, string apiKey, string endpointPrefix = "/api/v2")
		{
			_hostUrl = hostUrl;
			_apiKey = apiKey;
			_endpointPrefix = endpointPrefix;
		}

        /// <inheritdoc />
        public async Task<IEnumerable<Ticket>> ListTicketsAsync()
            => await ExecuteAsync<List<Ticket>>("/tickets", Method.GET);

        /// <inheritdoc />
        public Task<Ticket> GetTicketAsync(long id)
            => ExecuteAsync<Ticket>($"/tickets/{id}", Method.GET);

        /// <inheritdoc />
        public Task UpdateTicketWorkItemAsync(long id, string value)
            => ExecuteAsync<Ticket>($"/tickets/{id}", Method.PUT, new UpdateTicketWorkItemRequest(value).ToJson());

        /// <summary>
        /// This affects our daily usage limit, so be aware
        /// </summary>
        private async Task<T> ExecuteAsync<T>(string resource, Method method, string content = null)
        {
            var client = GetClient();
            var request = GetRequest(resource, method);

            if (!content.IsNullOrEmpty())
            {
                request.AddParameter("application/json", content, ParameterType.RequestBody);
            }

            var response = await client.ExecuteTaskAsync<T>(request);

            if (!response.IsSuccessful) throw new Exception(response.StatusDescription);

            try
            {
                return JsonConvert.DeserializeObject<T>(response.Content);
            }
            catch (Exception exc)
            {
                // response.Content should be logged somewhere maybe
                throw new Exception($"API call to {resource} succeeded, but Json deserialization of the result failed. {exc.Message}", exc);
            }
        }

        private RestClient GetClient()
		{
			var client = new RestClient(_hostUrl)
			{
				Authenticator = new HttpBasicAuthenticator(_apiKey, "X")
			};

			return client;
		}

		private RestRequest GetRequest(string resource, Method method)
		{
			return new RestRequest(_endpointPrefix + resource, method);
		}
	}
}