using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Ginseng.Mvc.Services
{
	/// <summary>
	/// thanks to https://www.jerriepelser.com/blog/authenticate-oauth-aspnet-core-2/
	/// </summary>
	internal static class GitHubLogin
	{
		internal static AuthenticationBuilder AddGitHub(this AuthenticationBuilder builder, IConfiguration config)
		{
			return builder.AddOAuth("GitHub", "GitHub", options =>
			{
				options.ClientId = config["GitHub:ClientId"];
				options.ClientSecret = config["GitHub:ClientSecret"];
				options.CallbackPath = new PathString("/signin-github");

				options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
				options.TokenEndpoint = "https://github.com/login/oauth/access_token";
				options.UserInformationEndpoint = "https://api.github.com/user";

				options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
				options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
				options.ClaimActions.MapJsonKey("urn:github:login", "login");
				options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
				options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");

				options.Events = new OAuthEvents
				{
					OnCreatingTicket = async context =>
					{
						var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
						request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
						request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

						var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
						response.EnsureSuccessStatusCode();

						var user = JObject.Parse(await response.Content.ReadAsStringAsync());

						context.RunClaimActions(user);
					}
				};
			});
		}
	}
}