using Ginseng.Models;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Ginseng.Mvc
{
	internal static class AerieHubLogin
	{
		internal static AuthenticationBuilder AddAerieHub(this AuthenticationBuilder builder, IConfiguration config)
		{
			const string TenantName = "AerieHub.com";

			return builder.AddOpenIdConnect("Microsoft", TenantName, options =>
			{
				options.SignInScheme = IdentityConstants.ExternalScheme;

				var tenant = TenantName;
				options.Authority = $"https://login.microsoftonline.com/{tenant}/v2.0";
				options.ClientId = config["AzureAd:ClientId"];
				options.ClientSecret = config["AzureAd:ClientSecret"];

				options.CallbackPath = new PathString("/signin-oidc");

				options.Scope.Clear();
				options.Scope.Add("openid");
				options.Scope.Add("profile");
				options.Scope.Add("email");
				options.ResponseType = "code";

				options.SaveTokens = true;
				options.GetClaimsFromUserInfoEndpoint = true;

				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = false, // set to true and populate ValidIssuers to only allow login from registered directories
					NameClaimType = "name"
				};

				options.Events = new OpenIdConnectEvents
				{
					OnTicketReceived = async (context) =>
					{
						var data = new DataAccess(config);
						using (var cn = data.GetConnection())
						{
							await OrganizationUser.ConnectPrincipalAsync(cn, context.Principal, TenantName);
						}
					}
				};
			});
		}
	}
}