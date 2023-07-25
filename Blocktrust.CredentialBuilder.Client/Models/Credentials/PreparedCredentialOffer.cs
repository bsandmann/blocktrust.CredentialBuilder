namespace Blocktrust.CredentialBuilder.Client.Models.Credentials;

using Connections;
using Dids;

public class PreparedCredentialOffer
{
    public EstablishedConnection EstablishedConnection { get; }
    public LocalDid IssuerDid { get; }
    public Dictionary<string, object> Claims { get; }
    public bool AutomaticIssuance { get; }
    public string? SchemaId { get; }
    public int? ValidityPeriod { get; }

    public PreparedCredentialOffer(EstablishedConnection establishedConnection, LocalDid issuerDid, Dictionary<string, object> claims, bool automaticIssuance, string? schemaId = null, int? validityPeriod = null)
    {
        EstablishedConnection = establishedConnection;
        IssuerDid = issuerDid;
        Claims = claims;
        AutomaticIssuance = automaticIssuance;
        SchemaId = schemaId;
        ValidityPeriod = validityPeriod;
    }
}