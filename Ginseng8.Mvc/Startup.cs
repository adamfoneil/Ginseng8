using AutoMapper;
using Ginseng.Mvc.Data;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Models.Freshdesk;
using Ginseng.Mvc.Services;
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
					Configuration.GetSection("ConnectionStrings").GetValue<string>("Default")));

			services
				.AddTransient<IUserStore<IdentityUser>, ExUserStore>()
				.AddTransient<IUserLoginStore<IdentityUser>, ExUserStore>()
				.AddDefaultIdentity<IdentityUser>(ConfigureIdentity)
				.AddDefaultUI(UIFramework.Bootstrap4)
				.AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration.GetSection("Google").GetValue<string>("ClientId");
                    options.ClientSecret = Configuration.GetSection("Google").GetValue<string>("ClientSecret");
                })
                .AddCookie()
                .AddAerieHub(Configuration)
				.AddGitHub(Configuration);

            services
                .AddOptions()
                .AddAutoMapper(typeof(Startup).Assembly)
                .AddSingleton<FreshdeskTicketCache>()
                .AddSingleton<FreshdeskGroupCache>()
                .AddSingleton<FreshdeskCompanyCache>()
                .AddSingleton<FreshdeskContactCache>()
                .AddSingleton<IFreshdeskClientFactory, FreshdeskClientFactory>()
                .AddSingleton<IFreshdeskService, FreshdeskService>()
                .Configure<FreshdeskServiceOptions>(Configuration.GetSection("Freshdesk"))
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
			options.User.RequireUniqueEmail = false;
		}
	}
}