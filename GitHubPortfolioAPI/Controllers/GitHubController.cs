using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Route("api/github")]
[ApiController]
public class GitHubController : ControllerBase
{
    private readonly IGitHubService _gitHubService;

    public GitHubController(GitHubService gitHubService)
    {
        _gitHubService = gitHubService;
    }

    [HttpGet("portfolio/{username}")]
    public async Task<IActionResult> GetPortfolio(string username)
    {
        var repos = await _gitHubService.GetUserRepositories(username);
        return Ok(repos);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchRepositories([FromQuery] string query, [FromQuery] string language, [FromQuery] string user)
    {
        var results = await _gitHubService.SearchRepositories(query, language, user);
        return Ok(results.Items);
    }
}
