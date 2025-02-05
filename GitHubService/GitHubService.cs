using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GitHubService
{
    private readonly GitHubClient _client;

    public GitHubService(string token)
    {
        _client = new GitHubClient(new ProductHeaderValue("GitHubPortfolioApp"))
        {
            Credentials = new Credentials(token)
        };
    }

    public async Task<IReadOnlyList<Repository>> GetUserRepositories(string username)
    {
        return await _client.Repository.GetAllForUser(username);
    }
    public async Task<SearchRepositoryResult> SearchRepositories(string query, string language = null, string user = null)
    {
        var request = new SearchRepositoriesRequest(query)
        {
            Language = ParseLanguage(language),
            User = user
        };

        return await _client.Search.SearchRepo(request);
    }
    public async Task<DateTimeOffset?> GetLastActivityAsync(string username)
    {
        var events = await _client.Activity.Events.GetAllUserReceived(username);
        return events.FirstOrDefault()?.CreatedAt;
    }
    private Octokit.Language? ParseLanguage(string? language)
    {
        if (string.IsNullOrEmpty(language))
            return null;

        if (Enum.TryParse<Octokit.Language>(language, true, out var lang))
        {
            return lang;
        }

        return null; 
    }


}
