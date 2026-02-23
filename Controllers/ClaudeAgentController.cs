using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace my_ai_agent_service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClaudeAgentController : ControllerBase
{
    private readonly IAuthorizationHeaderProvider _headerProvider;
    private readonly IConfiguration _configuration;

    public ClaudeAgentController(IAuthorizationHeaderProvider headerProvider, IConfiguration configuration)
    {
        _headerProvider = headerProvider;
        _configuration = configuration;
    }

    [HttpGet(Name = "ClaudeAgent")]
    public async Task<AgentResponse> Get()
    {

        AgentResponse response = new AgentResponse
        {
            Message = "Hello from Claude Agent!",
            Token = await GetAuthorizationHeaderAsync(),
            Time = DateTime.UtcNow.ToString()
        };

        return response;
    }

    private async Task<string> GetAuthorizationHeaderAsync()
    {
        // Configure options for the agent identity
        string agentIdentity = _configuration["AgentIdentity:ID"] ?? throw new InvalidOperationException("AgentId configuration is missing.");
        var options = new AuthorizationHeaderProviderOptions()
            .WithAgentIdentity(agentIdentity);

        // Acquire an access token for the agent identity
        var authHeader = await _headerProvider.CreateAuthorizationHeaderForAppAsync(
            "https://graph.microsoft.com/.default", options);

        return authHeader;
    }
}

public class AgentResponse
{
    public string Message { get; set; }
    public string Token { get; set; }
    public string Time { get; set; }
}
