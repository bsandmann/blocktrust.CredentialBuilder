namespace Blocktrust.CredentialBuilder.Client.Models.Presentations;

using PrismAgentApi.Model;

public class CreatedPresentationStatus
{
    public PresentationStatus.StatusEnum Status { get; set; }
    public Guid PresentationId { get; set; }
    // proofs
    public List<string> CredentialData { get; set; }
    public Guid? ConnectionId { get; set; }

    // Used on the holder side
    public CreatedPresentationStatus(PresentationStatus.StatusEnum status, Guid? connectionId, Guid presentationId)
    {
        Status = status;
        PresentationId = presentationId;
        ConnectionId = connectionId;
    }
    
    // Used on the verifier side
    public CreatedPresentationStatus(PresentationStatus.StatusEnum status, Guid? connectionId, Guid presentationId, List<string> credentialData)
    {
        Status = status;
        PresentationId = presentationId;
        ConnectionId = connectionId;
        CredentialData = credentialData;
    }

    /// <summary>
    /// Currently the connectionId is not set when the accepted presentation is received on the verifier side.
    /// </summary>
    /// <param name="connectionId"></param>
    public void SetConnectionId(Guid connectionId)
    {
        ConnectionId = connectionId;
    }
    
}