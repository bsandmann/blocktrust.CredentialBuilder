namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using Models.Credentials;
using Models.Dids;
using Models.Presentations;
using PrismAgentApi.Client;
using PrismAgentApi.Model;

public class PresentationService : IPresentationService
{
    public async Task<Result<CreatedPresentationRequest>> CreatePresentationRequest(Agent agent, PreparedPresentationRequest presentationRequest)
    {
        Blocktrust.PrismAgentApi.Api.PresentProofApi presentProofApi = new Blocktrust.PrismAgentApi.Api.PresentProofApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));
        try
        {
            var response = await presentProofApi.RequestPresentationAsync(
                new RequestPresentationInput(
                    connectionId: presentationRequest.EstablishedConnection.ConnectionId.ToString(),
                    proofs: new List<ProofRequestAux>(),
                    options: new Options(
                        challenge: Guid.NewGuid().ToString(),
                        domain: agent.AgentInstanceUri.AbsoluteUri)
                ));
            var createdPresentationRequest = new CreatedPresentationRequest(
                presentationId: response.PresentationId
            );
            return Result.Ok(createdPresentationRequest);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }
}