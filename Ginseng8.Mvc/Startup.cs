using Ginseng.Mvc.Data;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Ginseng.Mvc
{
    public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(
					Configuration.GetSection("ConnectionStrings").GetValue<string>("Default")
					/*Configuration.GetConnectionString("DefaultConnection")*/));
			services.AddDefaultIdentity<IdentityUser>(ConfigureIdentity)
				.AddDefaultUI(UIFramework.Bootstrap4)
				.AddEntityFrameworkStores<ApplicationDbContext>();
           
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration.GetSection("Google").GetValue<string>("ClientId");
                    options.ClientSecret = Configuration.GetSection("Google").GetValue<string>("ClientSecret");
                })
                .AddCookie()
                .AddOpenIdConnect("Microsoft", "Microsoft/O365", options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;

                    var tenant = "common"; // allow both personal and org identities
                    options.Authority = $"https://login.microsoftonline.com/{tenant}/v2.0";
                    options.ClientId = Configuration["AzureAd:ClientId"];
                    options.ClientSecret = Configuration["AzureAd:ClientSecret"];

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
                        OnTicketReceived = (context) =>
                        {
                            // Demo how to intercept an incoming user
                            var claims = context.Principal.Claims;
                            return Task.CompletedTask;
                        }
                    };
                });

            services
                .AddTransient<IEmailSender, Email>()
                .AddSingleton<IViewRenderService, ViewRenderService>();

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
		}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();				
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();

			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
			});
		}

        private void ConfigureIdentity(IdentityOptions options)
        {
            // Requiring a confirmed email breaks external logins.
            //options.SignIn.RequireConfirmedEmail = true;
        }
    }
}