using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using ModelSync.Library.Extensions;
using System;
using System.IO;

namespace Ginseng.Mvc
{
	public class Program
	{
		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyLoad += DataModelHelper.Export;

			CreateWebHostBuilder(args).Build().Run();			
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((hostingContext, config) =>
				{										
					config.AddJsonFile("config.json");
					AddOptionalConfig(hostingContext, config, "aerie.json");
					
				})
				.UseStartup<Startup>();

		private static void AddOptionalConfig(WebHostBuilderContext hostingContext, IConfigurationBuilder config, string fileName)
		{
			string aerieConfig = Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, fileName);
			if (File.Exists(aerieConfig)) config.AddJsonFile(fileName);
		}
	}
}