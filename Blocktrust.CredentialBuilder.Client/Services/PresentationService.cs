namespace Blocktrust.CredentialBuilder.Client.Services;

using System.Text;
using FluentResults;
using Models;
using Models.Credentials;
using Models.Dids;
using Models.Presentations;
using Newtonsoft.Json;
using PrismAgentApi.Client;
using PrismAgentApi.Model;

public class PresentationService : IPresentationService
{
    private readonly HttpClient _httpClient;

    public PresentationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

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
                    proofs: new List<ProofRequestAux>()
                    {
                        // new ProofRequestAux(schemaId: "be2ff82d-1fe6-4e2f-9dd7-6a867fe9534A",
                        //     trustIssuers: new List<string>(){"did:prism:23474572306bc10f4f289d4d3a737fa6bd79e5b4081409f83ca16390a1c3cb6f"})
                    },
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

    public async Task<Result<List<CreatedPresentationStatus>>> GetListPresentationRequests(Agent agent, PresentationStatus.StatusEnum? presentationStatus)
    {
        Blocktrust.PrismAgentApi.Api.PresentProofApi presentProofApi = new Blocktrust.PrismAgentApi.Api.PresentProofApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));
        try
        {
            var response = await presentProofApi.GetAllPresentationAsync();
            //TODO paging!
            var listReceivedPresentationRequests = new List<CreatedPresentationStatus>();
            IEnumerable<PresentationStatus> filteredList;
            // note: it is currently not possible to filter by any kind of date/time. We just can list all
            // requests. We also cannot filter by requests which have been made by connections we already saved
            // locally. BUG in PRISM

            var existingConnections = agent.Connections.Connections.Select(p => p.ConnectionId.ToString()).ToList();
            if (presentationStatus is not null)
            {
                filteredList = response.Contents
                    .Where(p => p.Status == presentationStatus); // &&
                // existingConnections.Contains(p.ConnectionId));
            }
            else
            {
                filteredList = response.Contents
                    .Where(p =>
                        p.Status == PresentationStatus.StatusEnum.PresentationSent ||
                        p.Status == PresentationStatus.StatusEnum.PresentationVerified ||
                        p.Status == PresentationStatus.StatusEnum.PresentationRejected);
            }

            foreach (var content in filteredList)
            {
                var receivedCredentialOffer = new CreatedPresentationStatus(
                    status: content.Status,
                    presentationId: Guid.Parse(content.PresentationId),
                    connectionId: string.IsNullOrEmpty(content.ConnectionId) ? null : Guid.Parse(content.ConnectionId)
                );
                listReceivedPresentationRequests.Add(receivedCredentialOffer);
            }

            return Result.Ok(listReceivedPresentationRequests);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result> AcceptRejectPresentationRequest(Agent agent, CreatedPresentationStatus presentation, Guid? credentialRecordId, bool acceptRequest)
    {
        // The auto-generated implementation of the PATCH-endpoint does not work. I get a exception because of some payload formatting.

        // OLD CODE: WHICH THROWS AN EXCEPTION (TODO test later again, after next NODE update. Also see the code below which is also accessing
        // the same PATCH endpoint)
        // Blocktrust.PrismAgentApi.Api.PresentProofApi presentProofApi = new Blocktrust.PrismAgentApi.Api.PresentProofApi(
        //     configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
        //         apiKey: new Dictionary<string, string>() { },
        //         apiKeyPrefix: new Dictionary<string, string>(),
        //         basePath: agent.AgentInstanceUri.AbsoluteUri));
        // try
        // {
        //     await presentProofApi.UpdatePresentationAsync(presentation.PresentationId, new RequestPresentationAction(
        //         RequestPresentationAction.ActionEnum.RequestAccept,
        //         proofId: new List<string>() { credentialRecordId.ToString() }
        //     ), CancellationToken.None);
        // }
        // catch (Exception e)
        // {
        //     return Result.Fail(e.Message);
        // }

        // NEW CODE: WHICH WORKS

        var payload = JsonConvert.SerializeObject(new RequestPresentationAction(
            acceptRequest ? RequestPresentationAction.ActionEnum.RequestAccept : RequestPresentationAction.ActionEnum.RequestReject,
            proofId: credentialRecordId is null? new List<string>() : new List<string>() { credentialRecordId!.ToString() }
        ));

        var url = string.Concat(agent.AgentInstanceUri.AbsoluteUri, "/present-proof/presentations/", presentation.PresentationId);
        try
        {
            // add headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apiKey", agent.AgentApiKey);
            var response = await _httpClient.PatchAsync(url, new StringContent(payload, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail(response.ReasonPhrase);
            }
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }

        return Result.Ok();
    }

    public async Task<Result<CreatedPresentationStatus>> WaitForPresentationRequestAcceptance(Agent agent, Guid presentationIdWaitingForAcceptance, CancellationToken cancellationToken)
    {
        int attempts = 0;
        var tcs = new TaskCompletionSource<Result<CreatedPresentationStatus>>();

        cancellationToken.Register(() => tcs.TrySetCanceled());

        Blocktrust.PrismAgentApi.Api.PresentProofApi presentProofApi = new Blocktrust.PrismAgentApi.Api.PresentProofApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));

        async Task OnTimerElapsedAsync(object state)
        {
            attempts++;
            var response = await presentProofApi.GetPresentationAsync(presentationIdWaitingForAcceptance, cancellationToken);
            if (response.Status == PresentationStatus.StatusEnum.PresentationRejected ||
                response.Status == PresentationStatus.StatusEnum.PresentationAccepted ||
                response.Status == PresentationStatus.StatusEnum.PresentationVerified)
            {
                var receivedPresentation = new CreatedPresentationStatus(
                    status: response.Status,
                    presentationId: Guid.Parse(response.PresentationId),
                    connectionId: response.ConnectionId is null ? null : Guid.Parse(response.ConnectionId),
                    credentialData: response.Data
                );
                tcs.TrySetResult(Result.Ok(receivedPresentation));
            }
            else if (attempts >= GlobalSettings.MaxAttempts)
            {
                tcs.TrySetResult(Result.Fail("timeout"));
            }
        }

        void OnTimerElapsed(object state)
        {
            // Call the async local function and ignore the returned task.
            _ = OnTimerElapsedAsync(state);
        }

        using Timer timer = new Timer(OnTimerElapsed, null, 0, GlobalSettings.Interval);

        try
        {
            return await tcs.Task;
        }
        finally
        {
            await timer.DisposeAsync();
        }
    }

    public async Task<Result<CreatedPresentationRequest>> AcceptRejectPresentation(Agent agent, Guid presentationId, bool acceptPresentation)
    {
        var payload = JsonConvert.SerializeObject(new RequestPresentationAction(
            acceptPresentation ? RequestPresentationAction.ActionEnum.PresentationAccept : RequestPresentationAction.ActionEnum.PresentationReject
        ));

        var url = string.Concat(agent.AgentInstanceUri.AbsoluteUri, "/present-proof/presentations/", presentationId.ToString());
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apiKey", agent.AgentApiKey);
            var response = await _httpClient.PatchAsync(url, new StringContent(payload, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail(response.ReasonPhrase);
            }
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }

        return Result.Ok();
    }
}