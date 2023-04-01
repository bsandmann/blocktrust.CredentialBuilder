namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using PrismAgentApi.Model;

public interface IConnectionService
{
    Task<Result<List<Connection>>> GetListOfConnections(Agent agent);
}