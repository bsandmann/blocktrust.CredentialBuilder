namespace Blocktrust.CredentialBuilder.Client.Models.Presentations;

using System.Text.Json.Serialization;

public class CreatedPresentationRequest
{
    [JsonPropertyName("rId")] public string PresentationId { get; set; }

    [JsonConstructor]
    public CreatedPresentationRequest()
    {
        
    }

    public CreatedPresentationRequest(string presentationId)
    {
       PresentationId = presentationId; 
    }
}