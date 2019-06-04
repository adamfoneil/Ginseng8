using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Ginseng.Mvc.Extensions;
using Ginseng.Mvc.Interfaces;
using Ginseng.Mvc.Options;
using Ginseng.Mvc.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Octokit;

namespace Testing
{
    /// <summary>
    /// <see cref="GitHubService"/> integration tests
    /// </summary>
    [TestClass]
    public class GitHubServiceTests
    {
        private readonly IConfiguration _config = new ConfigurationBuilder().AddJsonFile("config.json").Build();

        [TestMethod]
        public async Task ListInstallationsAsync_returns_list_of_app_installations()
        {
            // setup

            var service = new ServiceCollection()
                .UseConfiguration(_config)
                .AddConfigurationOptions<GitHubServiceOptions>()
                .AddSingleton<IGitHubService, GitHubService>()
                .AddSingleton((new Mock<IDataAccess>()).Object)
                .BuildServiceProvider()
                .GetService<IGitHubService>();

            // test

            var installations = await service.ListInstallationsAsync();
            installations.Should().NotBeNullOrEmpty("it's installed for tests in one or more GitHub accounts");
        }

        [TestMethod]
        public async Task ListRepositoriesAsync_returns_list_of_repositories_across_all_installations()
        {
            // setup

            var service = new ServiceCollection()
                .UseConfiguration(_config)
                .AddConfigurationOptions<GitHubServiceOptions>()
                .AddSingleton<IGitHubService, GitHubService>()
                .AddSingleton((new Mock<IDataAccess>()).Object)
                .BuildServiceProvider()
                .GetService<IGitHubService>();

            // test

            var repositories = await service.ListRepositoriesAsync();
            repositories.Should().NotBeNullOrEmpty("it's installed for tests in one or more GitHub accounts with at least one repository access");
        }

        [TestMethod]
        public async Task ListBranchesAsync_returns_list_of_branches_for_repository()
        {
            // setup

            var service = new ServiceCollection()
                .UseConfiguration(_config)
                .AddConfigurationOptions<GitHubServiceOptions>()
                .AddSingleton<IGitHubService, GitHubService>()
                .AddSingleton((new Mock<IDataAccess>()).Object)
                .BuildServiceProvider()
                .GetService<IGitHubService>();

            // test

            var repositories = await service.ListRepositoriesAsync();
            repositories.Should().NotBeNullOrEmpty("it's installed for tests in one or more GitHub accounts with at least one repository access");

            var branches = await service.ListBranchesAsync(repositories.First().Id);
            branches.Should().Contain(b => b.Name == "master", "master branch should exist in a repository");
        }
    }
}
