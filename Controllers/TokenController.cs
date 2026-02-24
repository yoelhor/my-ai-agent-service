using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;

namespace my_ai_agent_service.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TokenController : ControllerBase
{
    private readonly IAuthorizationHeaderProvider _headerProvider;
    private readonly IConfiguration _configuration;

    public TokenController(IAuthorizationHeaderProvider headerProvider, IConfiguration configuration)
    {
        _headerProvider = headerProvider;
        _configuration = configuration;
    }

    [HttpGet(Name = "GetToken")]
    public async Task<TokenResponse> Get()
    {

        TokenResponse response = new TokenResponse
        {
            Message = $"On-behalf-of token acquired successfully for agent identity '{_configuration["AgentIdentity:ID"]}'",
            Token = await GetAuthorizationHeaderAsync(),
            Time = DateTime.UtcNow.ToString()
        };

        return response;
    }

    private async Task<string> GetAuthorizationHeaderAsync()
    {
        // Configure options for the agent identity
        string agentIdentity = _configuration["AgentIdentity:ID"] ?? throw new InvalidOperationException("AgentId configuration is missing.");

        // Initialize options for agent identity
        var options = new AuthorizationHeaderProviderOptions()
            .WithAgentIdentity(agentIdentity);

        // Acquire an access token for the signed-in user with the specified scopes
        var authHeader = await _headerProvider.CreateAuthorizationHeaderForUserAsync(
            new[] { "User.Read" }, options);

        return authHeader.Replace("Bearer ", ""); // Return only the token without the "Bearer " prefix
    }
}

public class TokenResponse
{
    public required string Message { get; set; }
    public required string Token { get; set; }
    public required string Time { get; set; }
}
