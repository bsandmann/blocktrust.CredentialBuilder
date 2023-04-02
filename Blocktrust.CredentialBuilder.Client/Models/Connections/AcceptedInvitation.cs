namespace Blocktrust.CredentialBuilder.Client.Models.Connections;

using System.Text.Json.Serialization;
using MudBlazor;
using Net.Codecrete.QrCodeGenerator;
using PrismAgentApi.Model;

public class AcceptedInvitation
{
    [JsonPropertyName("cId")] public Guid ConnectionId { get; }
    [JsonPropertyName("id")] public Guid InvitationId { get; }
    [JsonPropertyName("lD")] public string LocalPeerDid { get; }
    [JsonPropertyName("rD")] public string RemotePeerDid { get; }
    [JsonPropertyName("s")] public Connection.StateEnum State { get; }
    [JsonPropertyName("u")] public DateTime UpdatedAt { get; }


    //Serialization
    public AcceptedInvitation()
    {
    }

    public AcceptedInvitation(Guid connectionId, Guid invitationId, string localPeerDid, string remotePeerDid, Connection.StateEnum state, DateTime updatedAt)
    {
        ConnectionId = connectionId;
        InvitationId = invitationId;
        LocalPeerDid = localPeerDid;
        RemotePeerDid = remotePeerDid;
        State = state;
        UpdatedAt = updatedAt;
    }
}