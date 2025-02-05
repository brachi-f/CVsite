using System.Collections.Frozen;
using Microsoft.Extensions.Options;
using Octokit;
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client;

        public GitHubService(IOptions<GitHubSettings> options)
        {
            _client = new GitHubClient(new ProductHeaderValue("GitHubPortfolioApp"))
            {
                Credentials = new Credentials(options.Value.Token)
            };
        }

        public async Task<IReadOnlyList<Repository>> GetUserRepositories(string username)
        {
            return await _client.Repository.GetAllForUser(username);
        }

        public async Task<List<RepositoryDetails>> GetRepositoryDetails(string username)
        {
            var repositories = await GetUserRepositories(username);
            var repositoryDetails = new List<RepositoryDetails>();

            foreach (var repo in repositories)
            {
                var pulls = await _client.PullRequest.GetAllForRepository(repo.Id);

                var lastCommit = repo.PushedAt;
                var pullRequestCount = pulls.Count();
                var stars = repo.StargazersCount;
                var languages = await _client.Repository.GetAllLanguages(repo.Id);

                repositoryDetails.Add(new RepositoryDetails
                {
                    Name = repo.Name,
                    LastCommit = lastCommit,
                    Stars = stars,
                    PullRequestCount = pullRequestCount,
                    Languages = languages.Select(l => l.Name),
                    Url = repo.HtmlUrl
                });
            }

            return repositoryDetails;
        }

        public async Task<SearchRepositoryResult> SearchRepositories(string query, string language, string user)
        {
            var request = new SearchRepositoriesRequest(query);

            if (!string.IsNullOrEmpty(language) && Enum.TryParse<Language>(language, true, out var parsedLanguage))
            {
                request.Language = parsedLanguage;
            }

            if (!string.IsNullOrEmpty(user))
            {
                request.User = user;
            }

            return await _client.Search.SearchRepo(request);
        }
    }

    public class RepositoryDetails
    {
        public string Name { get; set; }
        public DateTimeOffset? LastCommit { get; set; }
        public int Stars { get; set; }
        public int PullRequestCount { get; set; }
        public IEnumerable<string> Languages { get; set; }
        public string Url { get; set; }
    }
