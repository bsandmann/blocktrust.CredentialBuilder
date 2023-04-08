namespace Blocktrust.CredentialBuilder.Client.Models.Credentials;

using System.Text.Json.Serialization;

public class Credentials
{
    [JsonPropertyName("cOffers")] public List<CreatedCredentialOffer> CreatedCredentialOffers { get; set; } = new List<CreatedCredentialOffer>();
}