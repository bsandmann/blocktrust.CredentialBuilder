namespace Blocktrust.CredentialBuilder.Client.Models;

using System.Text.Json.Serialization;

public class Agent
{
    [JsonPropertyName("id")] public Guid AgentId { get; }
    [JsonPropertyName("uri")] public Uri AgentInstanceUri { get; }

    [JsonPropertyName("apiKey")] public string AgentApiKey { get; }

    [JsonPropertyName("name")] public string AgentName { get; }

    public Agent(Uri agentInstanceUri, string agentApiKey, string agentName)
    {
        AgentId = Guid.NewGuid();
        AgentInstanceUri = agentInstanceUri;
        AgentApiKey = agentApiKey;
        AgentName = agentName;
    }
}