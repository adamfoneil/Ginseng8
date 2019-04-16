using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
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
		public static void SaveHtml(this IBody record, IDbConnection connection)
		{
			string html = new HtmlSanitizer().Sanitize(record?.HtmlBody);

			if (connection != null)
			{
				html = ParseWorkItems(html, connection);
			}
			

			record.HtmlBody = html;
			record.TextBody = new Converter().Convert(record?.HtmlBody);
		}

		private static string ParseWorkItems(string html, IDbConnection connection)
		{
			var matches = Regex.Matches(html, @"#(\d*)").OfType<Match>();
			foreach (var match in matches)
			{

			}
		}
	}
}