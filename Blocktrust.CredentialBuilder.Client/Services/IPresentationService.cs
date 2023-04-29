namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using Models.Credentials;
using Models.Presentations;
using PrismAgentApi.Model;

public interface IPresentationService
{
    Task<Result<CreatedPresentationRequest>> CreatePresentationRequest(Agent agent, PreparedPresentationRequest presentationRequest);

    Task<Result<List<CreatedPresentationStatus>>> GetListPresentationRequests(Agent agent, PresentationStatus.StatusEnum? presentationStatus);
    Task<Result> AcceptRejectPresentationRequest(Agent agent, CreatedPresentationStatus presentation, Guid? credentialRecordId, bool acceptRequest);
    Task<Result<CreatedPresentationStatus>> WaitForPresentationRequestAcceptance(Agent agent, Guid presentationIdWaitingForAcceptance, CancellationToken cancellationToken);
    Task<Result<CreatedPresentationRequest>> AcceptRejectPresentation(Agent agent, Guid presentationId, bool acceptPresentation);
}