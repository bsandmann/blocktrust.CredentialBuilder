namespace Blocktrust.CredentialBuilder.Client.Models;

using System.Text.Json.Serialization;
using Connections;
using Credentials;
using Dids;

public class Agent
{
    [JsonPropertyName("id")] public Guid AgentId { get; set; }
    [JsonPropertyName("uri")] public Uri AgentInstanceUri { get; set; }

    [JsonPropertyName("apiKey")] public string AgentApiKey { get; set; }

    [JsonPropertyName("name")] public string AgentName { get; set; }

    [JsonPropertyName("conn")] public EstablishedConnections Connections { get; set; } = new EstablishedConnections();
    [JsonPropertyName("dids")] public LocalDids LocalDids { get; set; } = new LocalDids();
    [JsonPropertyName("c")] public Credentials.Credentials Credentials { get; set; } = new Credentials.Credentials();

    [JsonConstructor]
    public Agent()
    {
    }

    public Agent(Uri agentInstanceUri, string agentApiKey, string agentName)
    {
        AgentId = Guid.NewGuid();
        AgentInstanceUri = agentInstanceUri;
        AgentApiKey = agentApiKey;
        AgentName = agentName;
    }

    public void AddConnection(EstablishedConnection connection)
    {
        Connections.Connections.Add(connection);
    }

    public void AddDid(LocalDid localDid)
    {
        LocalDids.Dids.Add(localDid);
    }

    public void AddCredentialOffer(CreatedCredentialOffer credentialOffer)
    {
        Credentials.CreatedCredentialOffers.Add(credentialOffer);
    }
}