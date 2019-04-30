using Ginseng.Models.Freshdesk;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
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

		public IEnumerable<Ticket> ListTickets()
		{
			return ExecuteGet<Ticket[]>("/tickets");
		}

		/// <summary>
		/// This affects our daily usage limit, so be aware
		/// </summary>
		private T ExecuteGet<T>(string resource)
		{
			var client = GetClient();
			var request = GetRequest(resource);
			var response = client.Get(request);
			if (response.IsSuccessful)
			{
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
			else
			{
				throw response.ErrorException;
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

		private RestRequest GetRequest(string resource)
		{
			return new RestRequest(_endpointPrefix + resource, Method.GET);
		}
	}
}