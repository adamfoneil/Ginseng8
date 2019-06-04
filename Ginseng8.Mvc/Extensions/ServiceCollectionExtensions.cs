using Ginseng.Mvc.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ginseng.Mvc.Extensions
{
    /// <summary>
    /// IServiceCollection extensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configuration root
        /// </summary>
        private static IConfiguration Configuration { get; set; }

        /// <summary>
        /// Uses passed <c>configuration</c> in <see cref="AddConfigurationOptions{TOptions}"/>
        /// </summary>
        /// <param name="services">IServiceCollection instance</param>
        /// <param name="configuration">Configuration root</param>
        /// <returns>IServiceCollection instance</returns>
        public static IServiceCollection UseConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            Configuration = configuration;
            return services;
        }

        /// <summary>
        /// Configures configuration options
        /// </summary>
        /// <typeparam name="TOptions">Configuration options type</typeparam>
        /// <param name="services">IServiceCollection instance</param>
        /// <param name="configuration">Configuration root, when <c>null</c> used the configuration passed in <see cref="UseConfiguration"/></param>
        /// <returns>IServiceCollection instance</returns>
        public static IServiceCollection AddConfigurationOptions<TOptions>(
            this IServiceCollection services,
            IConfiguration configuration = null)
            where TOptions : class, IConfigurationOptions
        {
            var sectionName = typeof(TOptions).Name
                .Replace("Options", "")
                .Replace("Service", "");

            var config = configuration ?? Configuration;
            var configSection = config.GetSection(sectionName);
            return services.Configure<TOptions>(configSection);
        }
    }
}
