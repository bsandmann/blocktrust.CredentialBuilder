namespace Blocktrust.CredentialBuilder.Client.Models.Credentials;

using Connections;
using Dids;

public class PreparedCredentialOffer
{
    public EstablishedConnection EstablishedConnection { get; }
    public LocalDid IssuerDid { get; }
    public string SubjectDid { get; }
    public Dictionary<string, string> Claims { get; }
    public bool AutomaticIssuance { get; }
    public string? SchemaId { get; }
    public int? ValidityPeriod { get; }

    public PreparedCredentialOffer(EstablishedConnection establishedConnection, LocalDid issuerDid, string subjectDid, Dictionary<string, string> claims, bool automaticIssuance, string? schemaId = null, int? validityPeriod = null)
    {
        EstablishedConnection = establishedConnection;
        IssuerDid = issuerDid;
        SubjectDid = subjectDid;
        Claims = claims;
        AutomaticIssuance = automaticIssuance;
        SchemaId = schemaId;
        ValidityPeriod = validityPeriod;
    }
}