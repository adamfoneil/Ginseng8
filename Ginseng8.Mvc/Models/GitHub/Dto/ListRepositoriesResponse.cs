using System.Collections.Generic;
using Octokit;

namespace Ginseng.Mvc.Models.GitHub.Dto
{
    public class ListRepositoriesResponse
    {
        public IEnumerable<Repository> Repositories { get; set; }
    }
}
