namespace Blocktrust.CredentialBuilder.Client.Models.Dids;

using global::System.Text.Json.Serialization;

public class LocalDids
{
    [JsonPropertyName("dids")] public List<LocalDid> Dids { get; set; } = new List<LocalDid>();
}