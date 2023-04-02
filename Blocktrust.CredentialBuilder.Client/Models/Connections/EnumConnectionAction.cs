namespace Blocktrust.CredentialBuilder.Client.Models.Connections;

public enum EnumConnectionAction
{
    None,
    InvitationGenerated,
    ReadyToAcceptInvitation,
    
    // The Inviter send the response
    ConnectionResponseSent,
    
    // The Invitee send the request
    ConnectionRequestSent,
    
    // The Invitee received the final response and the connection is established
    ConnectionResponseReceived,
   
    // Failure
    TimeoutOrFailed,
}