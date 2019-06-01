using Microsoft.AspNetCore.Http;

namespace Ginseng.Mvc.Helpers
{
    public static partial class HtmlHelpers
    {
        public static string BuildUrl(this HttpRequest request, string append = null)
        {
            if (!append?.StartsWith("/") ?? false) append = "/" + append;
            return $"{request.Scheme}://{request.Host}{request.PathBase}{append}";
        }
    }
}