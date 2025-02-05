using Microsoft.Extensions.Caching.Memory;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CachedGitHubService : IGitHubService
{
    private readonly IMemoryCache _cache;
    private readonly IGitHubService _gitHubService;
    private readonly GitHubClient _client;

    public CachedGitHubService(IGitHubService gitHubService, IMemoryCache memoryCache, GitHubClient client)
    {
        _gitHubService = gitHubService;
        _cache = memoryCache;
        _client = client;
    }

    public Task<List<RepositoryDetails>> GetRepositoryDetails(string username)
    {
        return _gitHubService.GetRepositoryDetails(username);
    }

    public async Task<IReadOnlyList<Repository>> GetUserRepositories(string username)
    {
        if (_cache.TryGetValue(username, out (IReadOnlyList<Repository> Repos, DateTime LastUpdated) cachedData))
        {
            DateTime lastCommitDate = await GetLastCommitDate(username);

            if (lastCommitDate > cachedData.LastUpdated)
            {
                _cache.Remove(username);
            }
            else
            {
                return cachedData.Repos;
            }
        }

        var repos = await _gitHubService.GetUserRepositories(username);
        DateTime latestCommit = await GetLastCommitDate(username);

        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        _cache.Set(username, (repos, latestCommit), cacheEntryOptions);

        return repos;
    }

    private async Task<DateTime> GetLastCommitDate(string username)
    {
        var events = await _client.Activity.Events.GetAllUserPerformed(username);
        var lastCommitEvent = events
            .Where(e => e.Type == "PushEvent")
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefault();

        return lastCommitEvent?.CreatedAt.UtcDateTime ?? DateTime.MinValue;
    }
}
