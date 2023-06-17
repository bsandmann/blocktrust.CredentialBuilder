using Blocktrust.CredentialBuilder.Client.Models;
using Blocktrust.CredentialBuilder.Client.Models.Presentations;
using Blocktrust.CredentialBuilder.Client.Services;
using Blocktrust.PrismAgentApi.Client;
using Blocktrust.PrismAgentApi.Model;
using FluentResults;

public class SchemaService : ISchemaService
{
    public async Task<Result> GetListSchemas(Agent agent)
    {
        Blocktrust.PrismAgentApi.Api.SchemaRegistryApi schemaRegistryApi = new Blocktrust.PrismAgentApi.Api.SchemaRegistryApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));
        try
        {
            var response = await schemaRegistryApi.LookupSchemasByQueryAsync();
            // var createdPresentationRequest = new CreatedPresentationRequest(
            //     presentationId: response.PresentationId
            // );
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }
}