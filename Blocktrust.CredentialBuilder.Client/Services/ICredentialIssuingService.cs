namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using Models.Credentials;
using Models.Dids;
using PrismAgentApi.Model;

public interface ICredentialIssuingService
{
    Task<Result<CreatedCredentialOffer>> CreateCredentialOffer(Agent agent, PreparedCredentialOffer preparedCredential);
    Task<Result<List<CreatedCredentialOffer>>> GetListCredentials(Agent agent,IssueCredentialRecord.ProtocolStateEnum expectedState, TimeSpan? timeSpanOfListing = null);

    Task<Result<CreatedCredentialOffer>> WaitForCredentialOfferAcceptance(Agent agent, Guid credentialRecordId, CancellationToken cancellationToken);

    Task<Result<CreatedCredentialOffer>> AcceptCredentialOffer(Agent agent, CreatedCredentialOffer createdCredentialOffer, LocalDid subjectDid);
}