namespace Blocktrust.CredentialBuilder.Client.Models.Schema;

public interface ISchemaAttribute
{
    public string? Description { get; init; }
    public bool? Required { get; init; }
}