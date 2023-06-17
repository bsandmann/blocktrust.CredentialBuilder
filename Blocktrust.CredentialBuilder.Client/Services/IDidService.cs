namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using Models.Dids;
using PrismAgentApi.Model;

public interface IDidService
{
    Task<Result<LocalDid>> CreateDid(Agent agent);
    Task<Result<LocalDid>> PublishDid(Agent agent, LocalDid localDid);
    Task<Result<string>> WaitForPublishedDid(Agent agent, LocalDid did, CancellationToken cancellationToken);
}