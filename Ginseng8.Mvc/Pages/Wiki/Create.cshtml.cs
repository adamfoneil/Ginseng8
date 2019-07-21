using Ginseng.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ginseng.Mvc.Pages.Wiki
{
    [Authorize]
    public class CreateModel : WikiPageModel
    {
        public CreateModel(IConfiguration config) : base(config)
        {
        }

        public async Task<IActionResult> OnPost(Article article)
        {
            article.OrganizationId = OrgId;
            await Data.TrySaveAsync(article);
            return RedirectToPage("Index", new { id = article.Id });
        }
    }
}