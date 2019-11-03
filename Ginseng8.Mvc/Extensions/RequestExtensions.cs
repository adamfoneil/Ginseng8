using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Extensions
{
	public static class RequestExtensions
	{
		public static async Task<string> ReadStringAsync(this HttpRequest request)
		{
			using (var reader = new StreamReader(request.Body))
			{
				return await reader.ReadToEndAsync();
			}
		}

		public static IHtmlContent ToHiddenFields(this HttpRequest request, params string[] except)
		{
			string[] fields = request.Query.Keys.Except(except ?? Enumerable.Empty<string>()).ToArray();
			string result = string.Join("\r\n", fields.Select(field => $"<input type=\"hidden\" name=\"{field}\" value=\"{request.Query[field]}\"/>"));
			return new HtmlString(result);
		}

        public static Dictionary<string, string> ToRouteData(this HttpRequest request, Dictionary<string, string> append = null)
        {
            var fields = request.Query.Keys.Except(append?.Select(kp => kp.Key) ?? Enumerable.Empty<string>());
            var result = fields.ToDictionary(field => field, field => request.Query[field].First());

            if (append != null)
            {
                foreach (var kp in append) result.Add(kp.Key, kp.Value);
            }

            return result;
        }
	}
}