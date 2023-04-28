namespace Blocktrust.CredentialBuilder.Client.Models.Presentations;

using Connections;

public class PreparedPresentationRequest
{
    public EstablishedConnection EstablishedConnection { get; }
    
    public PreparedPresentationRequest(EstablishedConnection establishedConnection)
    {
        EstablishedConnection = establishedConnection;
    }
}