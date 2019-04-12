using System.Threading.Tasks;

namespace Ginseng.Mvc.Interfaces
{
    /// <summary>
    /// Razor template render to string service
    /// </summary>
    public interface IViewRenderService
    {
        /// <summary>
        /// Renders Razor template to a string
        /// </summary>
        /// <remarks>
        /// Razor templates ".cshtml" files must be configured as Embedded resources to the project
        /// </remarks>
        /// <typeparam name="T">Template's model</typeparam>
        /// <param name="template">Path to template (dots separated, e.g. "Pages.EmailContent.JoinRequest")</param>
        /// <param name="model">Model</param>
        /// <returns>Rendered to string template</returns>
        Task<string> RenderAsync<T>(string template, T model);
    }
}
