using Microsoft.Extensions.Caching.Memory;
using Octokit;
using System;

public class CachedGitHubService : GitHubService
{
    private readonly IMemoryCache _cache;

    public CachedGitHubService(IMemoryCache cache, string token) : base(token)
    {
        _cache = cache;
    }

    public async Task<IReadOnlyList<Repository>> GetUserRepositoriesWithCache(string username)
    {
        if (!_cache.TryGetValue(username, out IReadOnlyList<Repository> cachedRepos))
        {
            cachedRepos = await base.GetUserRepositories(username);
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            _cache.Set(username, cachedRepos, cacheEntryOptions);
        }

        return cachedRepos;
    }
    

}
