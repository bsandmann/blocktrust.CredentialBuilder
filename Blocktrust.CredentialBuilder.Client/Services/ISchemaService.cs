namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using PrismAgentApi.Model;

public interface ISchemaService
{
    Task<Result<List<CredentialSchemaResponse>>> GetListSchemas(Agent agent, string? author = null, string? name = null, string? version = null, string? tags = null);
}