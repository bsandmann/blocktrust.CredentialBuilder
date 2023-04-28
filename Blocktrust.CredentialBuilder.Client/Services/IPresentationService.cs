namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using Models.Presentations;

public interface IPresentationService
{
    Task<Result<CreatedPresentationRequest>> CreatePresentationRequest(Agent agent, PreparedPresentationRequest presentationRequest);
}