namespace Blocktrust.CredentialBuilder.Client.Models;

using global::System.Text.Json.Serialization;

public class Agents
{
    [JsonPropertyName("a")] public List<Agent> List { get; set; } = new List<Agent>();
}