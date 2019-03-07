using Ganss.XSS;
using Ginseng.Models.Interfaces;
using Html2Markdown;

namespace Ginseng.Mvc.Helpers
{
	public static class HtmlContent
	{
		/// <summary>
		/// Sanitizes HTML content and converts markdown on forms bound to IBody
		/// </summary>		
		public static void SaveHtml(this IBody record)
		{
			record.HtmlBody = new HtmlSanitizer().Sanitize(record?.HtmlBody);
			record.TextBody = new Converter().Convert(record?.HtmlBody);
		}
	}
}