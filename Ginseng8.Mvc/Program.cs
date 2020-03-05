using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using ModelSync.Library.Attributes;
using ModelSync.Library.Extensions;
using ModelSync.Library.Services;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ginseng.Mvc
{
	public class Program
	{
		public static void Main(string[] args)
		{
			//var a = Assembly.LoadFrom(@"C:\Users\Adam\Source\Repos\Ginseng8\Ginseng8.Mvc\bin\Debug\netcoreapp2.2\Ginseng.Models.dll");
			//var a = Assembly.LoadFrom(@"Ginseng.Models.dll");
			//var assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.Location).ToArray();
			AppDomain.CurrentDomain.AssemblyLoad += AssemblyHelper2.ExportDataModel;

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

	public static class AssemblyHelper2
	{
		private static void ExportDataModel(Assembly assembly, string defaultSchema, string defaultIdentityColumn)
		{
			// need a try block and error log here
			var dataModel = new AssemblyModelBuilder().GetDataModel(assembly, defaultSchema, defaultIdentityColumn);
			string json = JsonConvert.SerializeObject(dataModel, Formatting.Indented);
			string outputFile = Path.Combine(Path.GetDirectoryName(assembly.Location), Path.GetFileNameWithoutExtension(assembly.Location) + ".DataModel.json");
			File.WriteAllText(outputFile, json);
		}

		/// <summary>
		/// Use this in your app's startup, bound to AppDomain.CurrentDomain.AssemblyLoad event to export data models
		/// from assemblies marked with the [ExportDataModel] attribute. This enables ModelSync to work around issues
		/// loading assemblies dynamically
		/// </summary>        
		public static void ExportDataModel(object sender, AssemblyLoadEventArgs e)
		{
			var attr = e.LoadedAssembly.GetCustomAttribute<ExportDataModelAttribute>();
			if (attr != null)
			{
				ExportDataModel(e.LoadedAssembly, attr.DefaultSchema, attr.DefaultIdentityColumn);
			}
		}
	}

}