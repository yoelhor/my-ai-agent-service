using Anthropic;
using Anthropic.Core;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using ModelContextProtocol.Client;

using System.Text.Json.Serialization;

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

    [HttpPost(Name = "ClaudeAgent")]
    public async Task<AgentResponse> Post([FromBody] ClaudeAgentRequest request)
    {
        var prompt = request.Prompt;

        var apiKey = _configuration["Providers:Claude:ApiKey"] ?? throw new InvalidOperationException("ANTHROPIC_API_KEY is not set.");
        var model = _configuration["Providers:Claude:Model"] ?? "claude-haiku-4-5";
        string mcpEndpoint = _configuration["MCP:Endpoint"] ?? "https://learn.microsoft.com/api/mcp";


        // Connect to an HTTP remote MCP server via Streamable HTTP
        await using McpClient mcpClient = await McpClient.CreateAsync(new HttpClientTransport(new()
        {
            Name = "MCPServer",
            Endpoint = new Uri(mcpEndpoint)
        }));


        // Retrieve the list of tools available on the MCP server
        var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

        Console.WriteLine("Creating Anthropic client and agent...");

        // Prompt the user to enter a request for the agent.
        Console.Write("Enter your request: ");

        // Check if the user entered a request, if not, use a default one.
        if (string.IsNullOrWhiteSpace(prompt))
        {
            prompt = "Tell me a joke.";
            Console.WriteLine($"No request entered. Using default request: {prompt}");
        }

        // Create an Anthropic client and wrap it as an AIAgent, providing the list of tools from the MCP server.
        AIAgent agent = new AnthropicClient(new ClientOptions { ApiKey = apiKey })
            .AsAIAgent(model: model,
                instructions: """
                Provide a concise and witty response to the user's request.
                Use the available tools if necessary to gather information or perform actions to enhance your response.
                When asked about the MCP version, use the "MCPServer" tool to retrieve the version information from the MCP server.
                """,
                name: "Joker",
                tools: [.. mcpTools]);

        Console.WriteLine("Invoking the agent...");

        // Invoke the agent and output the text result.
        var agentResponseStr = await agent.RunAsync(prompt);
        // Console.WriteLine(response);

        // Invoke the agent with streaming support.
        // await foreach (var update in agent.RunStreamingAsync(prompt))
        // {
        //     Console.Write(update);
        // }

        //Token = await GetAuthorizationHeaderAsync(),
            
        AgentResponse response = new AgentResponse
        {
            Message = "" + agentResponseStr,
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

        return authHeader;
    }
}

public class AgentResponse
{
    public required string Message { get; set; }
    public required string Time { get; set; }
}

public class ClaudeAgentRequest
{
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }
}
