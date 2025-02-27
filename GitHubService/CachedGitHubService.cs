﻿using Microsoft.Extensions.Caching.Memory;
using Octokit;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CachedGitHubService : IGitHubService
{
    private readonly IMemoryCache _cache;

    public CachedGitHubService(IMemoryCache cache, IOptions<GitHubSettings> options)
        : base(options)
    {
        _cache = cache;
    }

    public async Task<IReadOnlyList<Repository>> GetUserRepositoriesWithCache(string username)
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

        var repos = await base.GetUserRepositories(username);
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

        return lastCommitEvent?.CreatedAt ?? DateTime.MinValue;
    }
}
