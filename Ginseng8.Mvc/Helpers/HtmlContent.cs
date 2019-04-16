using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using Ganss.XSS;
using Ginseng.Models.Interfaces;
using Ginseng.Mvc.Services;
using Html2Markdown;

namespace Ginseng.Mvc.Helpers
{
	public static class HtmlContent
	{
		/// <summary>
		/// Sanitizes HTML content and converts markdown on forms bound to IBody.
		/// Also parses links to internal objects
		/// </summary>		
		public static async Task SaveHtmlAsync(this IBody record, DataAccess dataAccess, IDbConnection connection = null)
		{
			string html = new HtmlSanitizer().Sanitize(record?.HtmlBody);

			// put all the parse methods here (i.e. for work items, projects, mentions).
			// currently we just have work item parsing
			async Task<string> CallParseMethodsAsync(string input, IDbConnection cn)
			{
				string result = input;
				result = await ParseWorkItemsAsync(input, dataAccess, cn);
				return result;
			};

			if (connection == null)
			{
				// no connection currently open, so we need to open one
				using (var cn = dataAccess.GetConnection())
				{
					html = await CallParseMethodsAsync(html, cn);
				}
			}
			else
			{
				// use the existing open connection
				html = await CallParseMethodsAsync(html, connection);
			}					
						
			record.HtmlBody = html;
			record.TextBody = new Converter().Convert(record?.HtmlBody);
		}

		private static async Task<string> ParseWorkItemsAsync(string html, DataAccess dataAccess, IDbConnection cn)
		{
			return await ParseLinksAsync(html, @"#(\d*)", async (match) =>
			{
				string number = match.Substring(1);
				var result = await cn.QuerySingleOrDefaultAsync<int>("SELECT 1 FROM [dbo].[WorkItem] WHERE [OrganizationId]=@orgId AND [Number]=@number", new { orgId = dataAccess.CurrentOrg.Id, number });
				return (result == 1);
			}, (match) =>
			{
				string number = match.Substring(1);
				return $"<a href=\"/WorkItem/View/{number}\">{number}</a>";
			});
		}

		private static async Task<string> ParseLinksAsync(string html, string regexPattern, Func<string, Task<bool>> validation, Func<string, string> replaceWith)
		{
			string result = html;

			var matches = Regex.Matches(html, regexPattern).OfType<Match>();
			foreach (var match in matches)
			{
				try
				{
					if (await validation.Invoke(match.Value))
					{
						string insert = replaceWith.Invoke(match.Value);
						result = result.Replace(match.Value, insert);
					}
				}
				catch 
				{
					// do nothing, we don't want exceptions here interefering with content save
				}
			}

			return result;
		}
	}
}