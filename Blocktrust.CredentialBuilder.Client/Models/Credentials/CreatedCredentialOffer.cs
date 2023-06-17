namespace Blocktrust.CredentialBuilder.Client.Models.Credentials;

using System.Text.Json.Serialization;
using PrismAgentApi.Model;

public class CreatedCredentialOffer
{
    [JsonPropertyName("rId")] public string RecordId { get; set; }
    [JsonPropertyName("pS")] public IssueCredentialRecordAllOf.ProtocolStateEnum ProtocolState { get; set; }
    [JsonPropertyName("iD")] public string IssuerDid { get; set; }
    [JsonPropertyName("sD")] public string SubjectDid { get; set; }
    [JsonPropertyName("c")] public Dictionary<string, string> Claims { get; set; }
    [JsonPropertyName("aI")] public bool AutomaticIssuance { get; }
    //[JsonPropertyName("sI")] public string? SchemaId { get; set; }
    [JsonPropertyName("vP")] public decimal? ValidityPeriod { get; set; }
    [JsonPropertyName("cUTC")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("jwt")] public string JwtCredential { get; set; }
    [JsonPropertyName("s")] public bool SavedLocally { get; set; }
    

    [JsonConstructor]
    public CreatedCredentialOffer()
    {
        
    }
    
    public CreatedCredentialOffer(string recordId, IssueCredentialRecordAllOf.ProtocolStateEnum protocolState, string issuerDid, string? subjectDid, Dictionary<string, string> claims, bool automaticIssuance, decimal? validityPeriod, DateTime createdAt,
        string jwtCredential, bool savedLocally)
    {
        RecordId = recordId;
        ProtocolState = protocolState;
        IssuerDid = issuerDid;
        SubjectDid = subjectDid;
        Claims = claims;
        AutomaticIssuance = automaticIssuance;
        //SchemaId = schemaId;
        ValidityPeriod = validityPeriod;
        CreatedAt = createdAt;
        JwtCredential = jwtCredential;
        SavedLocally = savedLocally;
    }
}