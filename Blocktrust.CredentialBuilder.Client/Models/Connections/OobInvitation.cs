namespace Blocktrust.CredentialBuilder.Client.Models.Connections;

using System.Text.Json.Serialization;
using Net.Codecrete.QrCodeGenerator;

public class OobInvitation
{
    [JsonPropertyName("url")] public string InvitationUrl { get; }
    [JsonPropertyName("id")] public Guid InvitationId { get; }
    [JsonPropertyName("lD")] public string LocalDid { get; }
    [JsonPropertyName("qr")] public string QrCodeSvg { get; }


    //Serialization
    public OobInvitation()
    {
    }


    public OobInvitation(string invitationUrl, Guid invitationId, string localDid)
    {
        InvitationUrl = invitationUrl;
        LocalDid = localDid;
        InvitationId = invitationId;
        var qrCode = QrCode.EncodeText(invitationUrl, QrCode.Ecc.Medium);
        QrCodeSvg = qrCode.ToSvgString(4);
    }
}