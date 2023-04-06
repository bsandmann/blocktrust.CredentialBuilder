namespace Blocktrust.CredentialBuilder.Client.Models.Connections;

using System.Text.Json.Serialization;

public class EstablishedConnection
{
    [JsonPropertyName("n")] public string? Label { get; }
    [JsonPropertyName("cId")] public Guid ConnectionId { get; }
    [JsonPropertyName("id")] public Guid InvitationId { get; }
    [JsonPropertyName("lD")] public string LocalPeerDid { get; }
    [JsonPropertyName("rD")] public string RemotePeerDid { get; }

    public EstablishedConnection(string localPeerDid, string remotePeerDid, Guid invitationId, Guid connectionId, string? label = null)
    {
        Label = label;
        ConnectionId = connectionId;
        InvitationId = invitationId;
        LocalPeerDid = localPeerDid;
        RemotePeerDid = remotePeerDid;
    }
}