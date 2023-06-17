namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using Models.Dids;
using PrismAgentApi.Client;
using PrismAgentApi.Model;

public class DidService : IDidService
{
    public async Task<Result<LocalDid>> CreateDid(Agent agent)
    {
        Blocktrust.PrismAgentApi.Api.DIDRegistrarApi didRegistrarApi = new Blocktrust.PrismAgentApi.Api.DIDRegistrarApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));
        try
        {
            var response = await didRegistrarApi.PostDidRegistrarDidsAsync((new CreateManagedDidRequest(
                new CreateManagedDidRequestDocumentTemplate(
                    new List<ManagedDIDKeyTemplate>()
                    {
                        new ManagedDIDKeyTemplate(id: "key-1", purpose: Purpose.Authentication),
                        new ManagedDIDKeyTemplate(id: "key-2", purpose: Purpose.KeyAgreement),
                        new ManagedDIDKeyTemplate(id: "key-3", purpose: Purpose.AssertionMethod),
                    }, new List<Service>()
                    {
                        new Service(id:"service-1", type: UpdateManagedDIDRequestActionsInnerUpdateService.TypeEnum.LinkedDomains.ToString(), serviceEndpoint: new List<string>() { "https://some.service" })
                    }))));
            return Result.Ok(new LocalDid(response.LongFormDid));
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<LocalDid>> PublishDid(Agent agent, LocalDid localDid)
    {
        Blocktrust.PrismAgentApi.Api.DIDRegistrarApi didRegistrarApi = new Blocktrust.PrismAgentApi.Api.DIDRegistrarApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));
        try
        {
            var response = await didRegistrarApi.PostDidRegistrarDidsDidrefPublicationsAsync(localDid.Did);
            localDid.Published();
            return Result.Ok(localDid);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<string>> WaitForPublishedDid(Agent agent, LocalDid did, CancellationToken cancellationToken)
    {
        int attempts = 0;
        var tcs = new TaskCompletionSource<Result<string>>();

        cancellationToken.Register(() => tcs.TrySetCanceled());

        Blocktrust.PrismAgentApi.Api.DIDRegistrarApi didRegistrarApi = new Blocktrust.PrismAgentApi.Api.DIDRegistrarApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));

        async Task OnTimerElapsedAsync(object state)
        {
            attempts++;
            var response = await didRegistrarApi.GetDidRegistrarDidsDidrefAsync(did.Did, cancellationToken);
            if (response.Status.Equals("PUBLISHED", StringComparison.CurrentCultureIgnoreCase))
            {
                tcs.TrySetResult(Result.Ok(response.Status));
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
}