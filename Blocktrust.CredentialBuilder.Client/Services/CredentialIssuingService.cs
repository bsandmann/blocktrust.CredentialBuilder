namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using Models.Credentials;
using PrismAgentApi.Client;
using PrismAgentApi.Model;

public class CredentialIssuingService : ICredentialIssuingService
{
    public async Task<Result<CreatedCredentialOffer>> CreateCredentialOffer(Agent agent, PreparedCredentialOffer preparedCredential)
    {
        Blocktrust.PrismAgentApi.Api.IssueCredentialsProtocolApi issueCredentialsProtocolApi = new Blocktrust.PrismAgentApi.Api.IssueCredentialsProtocolApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));
        try
        {
            var createCredentialOfferResponse = await issueCredentialsProtocolApi.CreateCredentialOfferAsync(
                new CreateIssueCredentialRecordRequest(
                    schemaId: preparedCredential.SchemaId,
                    subjectId: preparedCredential.SubjectDid,
                    validityPeriod: 0M,
                    claims: preparedCredential.Claims,
                    automaticIssuance: preparedCredential.AutomaticIssuance,
                    issuingDID: preparedCredential.IssuerDid.Did,
                    connectionId: preparedCredential.EstablishedConnection.ConnectionId.ToString()));
            var createdCredential = new CreatedCredentialOffer(
                recordId: createCredentialOfferResponse.RecordId,
                protocolState: createCredentialOfferResponse.ProtocolState,
                issuerDid: preparedCredential.IssuerDid.Did, // ATTENTION: This value currently comes from the prepared credential, but it should come from the response (I believe)
                subjectDid: preparedCredential.SubjectDid, // ATTENTION: This value currently comes from the prepared credential, but it should come from the response (I believe)
                claims: createCredentialOfferResponse.Claims,
                automaticIssuance: createCredentialOfferResponse.AutomaticIssuance,
                schemaId: createCredentialOfferResponse.SchemaId,
                validityPeriod: createCredentialOfferResponse.ValidityPeriod,
                createdAt: createCredentialOfferResponse.CreatedAt,
                jwtCredential: createCredentialOfferResponse.JwtCredential);
            return Result.Ok(createdCredential);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<List<CreatedCredentialOffer>>> GetListCredentialOffers(Agent agent, IssueCredentialRecord.ProtocolStateEnum expectedState, TimeSpan? maxLifetime = null)
    {
        Blocktrust.PrismAgentApi.Api.IssueCredentialsProtocolApi issueCredentialsProtocolApi = new Blocktrust.PrismAgentApi.Api.IssueCredentialsProtocolApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));
        try
        {
            var getCredentialRecordsResponse = await issueCredentialsProtocolApi.GetCredentialRecordsAsync();
            //TODO paging!
            var listReceivedCredentialOffers = new List<CreatedCredentialOffer>();
            IEnumerable<IssueCredentialRecord> filteredList;
            if (maxLifetime is null)
            {
                filteredList = getCredentialRecordsResponse.Contents
                    .Where(p => p.ProtocolState == expectedState)
                    .OrderByDescending(p => p.CreatedAt);
            }
            else
            {
                filteredList = getCredentialRecordsResponse.Contents
                    .Where(p => p.ProtocolState == expectedState && p.CreatedAt > DateTime.UtcNow - maxLifetime)
                    .OrderByDescending(p => p.CreatedAt);
            }

            foreach (var content in filteredList)
            {
                var receivedCredentialOffer = new CreatedCredentialOffer(
                    recordId: content.RecordId,
                    protocolState: content.ProtocolState,
                    issuerDid: content.IssuingDID,
                    subjectDid: content.SubjectId,
                    claims: content.Claims,
                    automaticIssuance: content.AutomaticIssuance,
                    schemaId: content.SchemaId,
                    validityPeriod: content.ValidityPeriod,
                    createdAt: content.CreatedAt,
                    jwtCredential: content.JwtCredential);
                listReceivedCredentialOffers.Add(receivedCredentialOffer);
            }

            return Result.Ok(listReceivedCredentialOffers);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<CreatedCredentialOffer>> WaitForCredentialOfferAcceptance(Agent agent, Guid credentialRecordId, CancellationToken cancellationToken)
    {
        int attempts = 0;
        var MaxAttempts = 20;
        const int Interval = 5000;
        var tcs = new TaskCompletionSource<Result<CreatedCredentialOffer>>();

        cancellationToken.Register(() => tcs.TrySetCanceled());

        Blocktrust.PrismAgentApi.Api.IssueCredentialsProtocolApi issueCredentialsProtocolApi = new Blocktrust.PrismAgentApi.Api.IssueCredentialsProtocolApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));

        async Task OnTimerElapsedAsync(object state)
        {
            attempts++;
            var credentialRecord = await issueCredentialsProtocolApi.GetCredentialRecordAsync(credentialRecordId, cancellationToken);
            var ff = credentialRecord.ProtocolState;
            if (credentialRecord.ProtocolState == IssueCredentialRecord.ProtocolStateEnum.CredentialSent)
            {
                // not really a nice solutions, since we have the created offer already in the db
                var receivedCredentialOffer = new CreatedCredentialOffer(
                    recordId: credentialRecord.RecordId,
                    protocolState: credentialRecord.ProtocolState,
                    issuerDid: credentialRecord.IssuingDID,
                    subjectDid: credentialRecord.SubjectId,
                    claims: credentialRecord.Claims,
                    automaticIssuance: credentialRecord.AutomaticIssuance,
                    schemaId: credentialRecord.SchemaId,
                    validityPeriod: credentialRecord.ValidityPeriod,
                    createdAt: credentialRecord.CreatedAt,
                    jwtCredential: credentialRecord.JwtCredential);
                tcs.TrySetResult(Result.Ok(receivedCredentialOffer));
            }
            else if (attempts >= MaxAttempts)
            {
                tcs.TrySetResult(Result.Fail("timeout"));
            }
        }

        void OnTimerElapsed(object state)
        {
            // Call the async local function and ignore the returned task.
            _ = OnTimerElapsedAsync(state);
        }

        using Timer timer = new Timer(OnTimerElapsed, null, 0, Interval);

        try
        {
            return await tcs.Task;
        }
        finally
        {
            await timer.DisposeAsync();
        }
    }

    public async Task<Result<CreatedCredentialOffer>> AcceptCredentialOffer(Agent agent, CreatedCredentialOffer createdCredentialOffer)
    {
        Blocktrust.PrismAgentApi.Api.IssueCredentialsProtocolApi issueCredentialsProtocolApi = new Blocktrust.PrismAgentApi.Api.IssueCredentialsProtocolApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));
        try
        {
            var isParsedAsGuid = Guid.TryParse(createdCredentialOffer.RecordId, out var recordId);
            if (!isParsedAsGuid)
            {
                return Result.Fail("Error parsing recordId as GUID");
            }

            //TODO BUG IN PRISM!
            createdCredentialOffer.SubjectDid = "did:prism:f32491bb9743a79d69018fa66cb34901687253759e701a3cf57f3fce74bf54cb";
            
            var acceptCredentialOfferAsync = await issueCredentialsProtocolApi.AcceptCredentialOfferAsync(recordId, new AcceptCredentialOfferRequest(createdCredentialOffer.SubjectDid));
            var createdCredential = new CreatedCredentialOffer(
                recordId: acceptCredentialOfferAsync.RecordId,
                protocolState: acceptCredentialOfferAsync.ProtocolState,
                issuerDid: acceptCredentialOfferAsync.IssuingDID,
                subjectDid: acceptCredentialOfferAsync.SubjectId,
                claims: acceptCredentialOfferAsync.Claims,
                automaticIssuance: acceptCredentialOfferAsync.AutomaticIssuance,
                schemaId: acceptCredentialOfferAsync.SchemaId,
                validityPeriod: acceptCredentialOfferAsync.ValidityPeriod,
                createdAt: acceptCredentialOfferAsync.CreatedAt,
                jwtCredential: acceptCredentialOfferAsync.JwtCredential);
            return Result.Ok(createdCredential);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }
}