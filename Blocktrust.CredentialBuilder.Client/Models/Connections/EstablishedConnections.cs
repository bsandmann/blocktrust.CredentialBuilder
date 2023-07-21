namespace Blocktrust.CredentialBuilder.Client.Models.Connections;

using global::System.Text.Json.Serialization;

public class EstablishedConnections
{
    [JsonPropertyName("c")] public List<EstablishedConnection> Connections { get; set; } = new List<EstablishedConnection>();
}