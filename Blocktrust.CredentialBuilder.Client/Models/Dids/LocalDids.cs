namespace Blocktrust.CredentialBuilder.Client.Models.Dids;

using System.Text.Json.Serialization;

public class LocalDids
{
    [JsonPropertyName("dids")] public List<LocalDid> Dids { get; set; } = new List<LocalDid>();
}