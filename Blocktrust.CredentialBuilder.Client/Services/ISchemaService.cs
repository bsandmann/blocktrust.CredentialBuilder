namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;

public interface ISchemaService
{
    Task<Result> GetListSchemas(Agent agent);
}