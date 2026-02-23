
# Agent Identity on-behalf-of web API sample

This readme walks you through building an agent app that invokes a web API on behalf of a signed-in user. The agent app authenticates the user, then passes the access token to a web API. The web API exchanges that incoming token for a new one scoped to the agent identity, which it then uses to call protected downstream resources, such as an MCP server.

# Step 1. Prepare your agent identities

Follow the guidance how to [Create an agent identity blueprint](https://learn.microsoft.com/en-us/entra/agent-id/identity-platform/create-blueprint?tabs=microsoft-graph-api) and [Create agent identities in agent identity](https://learn.microsoft.com/en-us/entra/agent-id/identity-platform/create-delete-agent-identities?tabs=microsoft-graph-api).

To authenticate a user, the agent app (such as a frontend or mobile app) should initiate an OAuth 2.0 authorization request to obtain a token where the audience is the agent identity blueprint. This requires the agent blueprint to have identifier URI and one scope (known as delegated permission). If you have not done so, [Configure identifier URI and scope](https://learn.microsoft.com/en-us/entra/agent-id/identity-platform/create-blueprint?tabs=microsoft-graph-api#configure-identifier-uri-and-scope) or your agent identity blueprint.

## Step 1.2 Consent for the agents scopes

Interactive agents that act on behalf of users must request delegated authorization from each user. However, as an admin, you can grant authorization on behalf of all users in your tenant, eliminating the need for individual user consent.

This is particularly important because without it, the web API may fail when attempting to exchange the user's token for a new one, since the user hasn't explicitly consented to the required permissions.

Follow the guidance in this article [Configure admin authorization for interactive agents](https://learn.microsoft.com/en-us/entra/agent-id/identity-platform/interactive-agent-request-admin-authorization). 

# Step 2. Register your application

To support the on-behalf-of flow, register your agent application in Microsoft Entra ID. This registration establishes a trust relationship between your app and the Microsoft identity platform, requiring users to sign in before they can access it. The agent app itself can be any type of client application, like web app, single-page application, mobile app, desktop app, or command-line (CLI), as long as it has a user interface through which users authenticate. Follow these steps:

1. [Register your applicaion](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app) and record its ID.
1. [Add a redirect URI to your application](https://learn.microsoft.com/en-us/entra/identity-platform/how-to-add-redirect-uri)
1. For confidential apps (like web app), [add application credentials](https://learn.microsoft.com/en-us/entra/identity-platform/how-to-add-credentials?tabs=certificate).

# Step 2.1 Configure app permissions

In the application registration, do the following:

1. Select **API permissions**, then **Add a permission** and select **My APIs** in the sidebar.
1. Select your agent identity blueprint.
1. From the list of permission, select the **access_agent**.
1. Select Add permissions to complete the process.
1. Select the **Grant admin consent for {your tenant}**, and then select **Yes**. It allows an admin to grant admin consent to the permissions configured for the application. 

# Step 3. Enable your agent app to sign-in

With your application registered, the next step is updating your app's code to enable user sign-in. The exact implementation varies depending on your app type. When configuring your app, use the following values:

- **Tenant ID** — your Microsoft Entra tenant ID.
- **Application ID (client ID)** — the ID of the application you registered. Note that this is neither the agent identity blueprint nor the agent identity.
- **Scope** — must follow this format: `api://<agent-identity-blueprint>/access_agent`. Replace `<agent-identity-blueprint>` with your actual agent identity blueprint ID.


# Step 4. Prepare you web API

Learn how to [Call custom APIs from an agent using .NET](https://learn.microsoft.com/en-us/entra/agent-id/identity-platform/call-api-custom?tabs=authheaderprovider)

# Sources

For more information:

- [Acquire user tokens for interactive agents](https://learn.microsoft.com/en-us/entra/agent-id/identity-platform/interactive-agent-request-user-tokens)
- [Microsoft.Identity.Web.AgentIdentities code sample](https://github.com/AzureAD/microsoft-identity-web/blob/master/docs/calling-downstream-apis/AgentIdentities-Readme.md)



