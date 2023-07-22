namespace Blocktrust.CredentialBuilder.Client.Models.Connections;

using global::System.Text.Json.Serialization;

public class EstablishedConnection
{
    [JsonPropertyName("n")] public string? Label { get; set;}
    [JsonPropertyName("cId")] public Guid ConnectionId { get; set; }
    [JsonPropertyName("id")] public Guid InvitationId { get; set; }
    [JsonPropertyName("lD")] public string LocalPeerDid { get; set; }
    [JsonPropertyName("rD")] public string RemotePeerDid { get; set;}

    [JsonConstructor]
    public EstablishedConnection()
    {
        
    }
    
    public EstablishedConnection(string localPeerDid, string remotePeerDid, Guid invitationId, Guid connectionId, string? label = null)
    {
        Label = label;
        ConnectionId = connectionId;
        InvitationId = invitationId;
        LocalPeerDid = localPeerDid;
        RemotePeerDid = remotePeerDid;
    }
}