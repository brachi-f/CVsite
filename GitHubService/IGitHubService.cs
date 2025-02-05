using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

    public interface IGitHubService
    {
        Task<IReadOnlyList<Repository>> GetUserRepositories(string username);
        Task<List<RepositoryDetails>> GetRepositoryDetails(string username);
    }

