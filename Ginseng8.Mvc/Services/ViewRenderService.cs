using System;
using System.Threading.Tasks;
using Ginseng.Mvc.Interfaces;
using RazorLight;

namespace Ginseng.Mvc.Services
{
    /// <summary>
    /// Razor template render to string service
    /// </summary>
    public class ViewRenderService : IViewRenderService
    {
        /// <summary>
        /// RazorLight engine
        /// </summary>
        private readonly IRazorLightEngine _engine;

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewRenderService()
        {
            _engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(Program))
                .UseMemoryCachingProvider()
                .Build();
        }

        /// <inheritdoc />
        public async Task<string> RenderAsync<T>(string template, T model)
        {
            if (!template.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
                template += ".cshtml";

            var result = await _engine.CompileRenderAsync(template, model);
            return result;
        }
    }
}
