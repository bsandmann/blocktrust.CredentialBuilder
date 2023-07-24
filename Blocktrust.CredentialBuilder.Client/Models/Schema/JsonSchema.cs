namespace Blocktrust.CredentialBuilder.Client.Models.Schema;

using PrismAgentApi.Model;

public class JsonSchema
{
    public Guid Guid { get; set; }
    public string Id { get; set; }
    public string LongId { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public List<string> Tags { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public JsonSchemaDefintion JsonSchemaDefintion { get; set; }

    public JsonSchema(CredentialSchemaResponse response)
    {
        Guid = response.Guid;
        Id = response.Id;
        LongId = response.LongId;
        Name = response.Name;
        Version = response._Version;
        Tags = response.Tags;
        Description = response.Description;
        Type = response.Type;
        JsonSchemaDefintion = new JsonSchemaDefintion(response.Schema);
    }
}