using Microsoft.AspNetCore.Http;
using System.IO;
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
	}
}