using Blocktrust.CredentialBuilder.Client.Models;
using Blocktrust.CredentialBuilder.Client.Models.Presentations;
using Blocktrust.CredentialBuilder.Client.Services;
using Blocktrust.PrismAgentApi.Client;
using Blocktrust.PrismAgentApi.Model;
using FluentResults;

public class SchemaService : ISchemaService
{
    public async Task<Result<List<CredentialSchemaResponse>>> GetListSchemas(Agent agent, string? author = null, string? name = null, string? version = null, string? tags = null)
    {
        Blocktrust.PrismAgentApi.Api.SchemaRegistryApi schemaRegistryApi = new Blocktrust.PrismAgentApi.Api.SchemaRegistryApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));
        try
        {
            //TODO pagination
            var response = await schemaRegistryApi.LookupSchemasByQueryAsync(author: author, name: name, version: version, tags: tags);
            return Result.Ok(response.Contents);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }
}