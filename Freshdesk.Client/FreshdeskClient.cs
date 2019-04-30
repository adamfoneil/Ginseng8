using Freshdesk.Client.Models;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;

namespace Ginseng.Integration.Services
{
	public class FreshdeskClient
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

		/// <summary>
		/// This affects our daily usage limit, so be aware
		/// </summary>
		private T ExecuteGet<T>(string resource)
		{
			var client = GetClient();
			var request = Get(resource);
			var response = client.Get(request);
			if (response.IsSuccessful)
			{
				return JsonConvert.DeserializeObject<T>(response.Content);
			}
			else
			{
				throw response.ErrorException;
			}
		}

		public IEnumerable<Ticket> ListTickets()
		{
			return ExecuteGet<Ticket[]>("/tickets");
		}

		private RestClient GetClient()
		{
			var client = new RestClient(_hostUrl)
			{
				Authenticator = new HttpBasicAuthenticator(_apiKey, "X")
			};

			return client;
		}

		private RestRequest Get(string resource)
		{
			return new RestRequest(_endpointPrefix + resource, Method.GET);
		}
	}
}