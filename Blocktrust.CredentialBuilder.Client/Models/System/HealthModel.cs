namespace Blocktrust.CredentialBuilder.Client.Models.System;

using global::System.Text.Json.Serialization;

public class HealthModel
{
    [JsonPropertyName("version")]
    public string Version { get; set; }
}