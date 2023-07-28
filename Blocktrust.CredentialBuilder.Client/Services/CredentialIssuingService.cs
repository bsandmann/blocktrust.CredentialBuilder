namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using Models.Credentials;
using Models.Dids;
using PrismAgentApi.Client;
using PrismAgentApi.Model;
using static Blocktrust.PrismAgentApi.Model.IssueCredentialRecordAllOf;

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
            var response = await issueCredentialsProtocolApi.CreateCredentialOfferAsync(
                new CreateIssueCredentialRecordRequest(
                    // schemaId: preparedCredential.SchemaId,
                    validityPeriod: (double)0M,
                    claims: preparedCredential.Claims,
                    automaticIssuance: preparedCredential.AutomaticIssuance,
                    issuingDID: preparedCredential.IssuerDid.Did,
                    connectionId: preparedCredential.EstablishedConnection.ConnectionId.ToString()));

            if (!Enum.TryParse<IssueCredentialRecordAllOf.ProtocolStateEnum>(response.ProtocolState, out var protocolStateEnum))
            {
                return Result.Fail("Error parsing string as ProtocolStateEnum");
            }

            var createdCredential = new CreatedCredentialOffer(
                recordId: response.RecordId,
                protocolState: protocolStateEnum,
                issuerDid: preparedCredential.IssuerDid.Did, // ATTENTION: This value currently comes from the prepared credential, but it should come from the response (I believe)
                subjectDid: null,
                claims: response.Claims,
                automaticIssuance: response.AutomaticIssuance,
                //schemaId: response.SchemaId,
                validityPeriod: (decimal?)response.ValidityPeriod,
                createdAt: response.CreatedAt,
                jwtCredential: Base64ToJwtDecoder(response.JwtCredential),
                savedLocally: false);
            return Result.Ok(createdCredential);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<List<CreatedCredentialOffer>>> GetListCredentials(Agent agent, IssueCredentialRecordAllOf.ProtocolStateEnum expectedState, TimeSpan? timeSpanOfListing = null)
    {
        Blocktrust.PrismAgentApi.Api.IssueCredentialsProtocolApi issueCredentialsProtocolApi = new Blocktrust.PrismAgentApi.Api.IssueCredentialsProtocolApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));
        try
        {
            var response = await issueCredentialsProtocolApi.GetCredentialRecordsAsync();

            //TODO paging!
            var listReceivedCredentialOffers = new List<CreatedCredentialOffer>();
            IEnumerable<IssueCredentialRecord> filteredList;
            if (timeSpanOfListing is null)
            {
                filteredList = response.Contents
                    .Where(p => string.Equals(p.ProtocolState, expectedState.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    .OrderByDescending(p => p.CreatedAt);
            }
            else
            {
                filteredList = response.Contents
                    .Where(p => string.Equals(p.ProtocolState, expectedState.ToString(), StringComparison.CurrentCultureIgnoreCase) && p.CreatedAt > DateTime.UtcNow - timeSpanOfListing)
                    .OrderByDescending(p => p.CreatedAt);
            }

            foreach (var content in filteredList)
            {
                if (!Enum.TryParse<IssueCredentialRecordAllOf.ProtocolStateEnum>(content.ProtocolState, out var protocolStateEnum))
                {
                    return Result.Fail("Error parsing string as ProtocolStateEnum");
                }

                var receivedCredentialOffer = new CreatedCredentialOffer(
                    recordId: content.RecordId,
                    protocolState: protocolStateEnum,
                    issuerDid: content.IssuingDID,
                    subjectDid: content.SubjectId,
                    claims: content.Claims,
                    automaticIssuance: content.AutomaticIssuance,
                    //schemaId: content.,
                    validityPeriod: (decimal?)content.ValidityPeriod,
                    createdAt: content.CreatedAt,
                    jwtCredential: Base64ToJwtDecoder(content.JwtCredential),
                    savedLocally: false);
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
            var response = await issueCredentialsProtocolApi.GetCredentialRecordAsync(credentialRecordId.ToString(), cancellationToken);

            if (!Enum.TryParse<IssueCredentialRecordAllOf.ProtocolStateEnum>(response.ProtocolState, out var protocolStateEnum))
            {
                return;
            }

            if (protocolStateEnum == IssueCredentialRecordAllOf.ProtocolStateEnum.CredentialSent)
            {
                // not really a nice solutions, since we have the created offer already in the db
                var receivedCredentialOffer = new CreatedCredentialOffer(
                    recordId: response.RecordId,
                    protocolState: protocolStateEnum,
                    issuerDid: response.IssuingDID,
                    subjectDid: response.SubjectId,
                    claims: response.Claims,
                    automaticIssuance: response.AutomaticIssuance,
                    //schemaId: response.SchemaId,
                    validityPeriod: (decimal?)response.ValidityPeriod,
                    createdAt: response.CreatedAt,
                    jwtCredential: Base64ToJwtDecoder(response.JwtCredential),
                    savedLocally: false);
                tcs.TrySetResult(Result.Ok(receivedCredentialOffer));
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

    public async Task<Result<CreatedCredentialOffer>> AcceptCredentialOffer(Agent agent, CreatedCredentialOffer createdCredentialOffer, LocalDid subjectDid)
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

            if (string.IsNullOrEmpty(createdCredentialOffer.SubjectDid))
            {
                createdCredentialOffer.SubjectDid = subjectDid.Did;
            }

            var response = await issueCredentialsProtocolApi.AcceptCredentialOfferAsync(recordId.ToString(), new AcceptCredentialOfferRequest(createdCredentialOffer.SubjectDid));

            if (!Enum.TryParse<IssueCredentialRecordAllOf.ProtocolStateEnum>(response.ProtocolState, out var protocolStateEnum))
            {
                return Result.Fail("Error parsing string as ProtocolStateEnum");
            }

            var createdCredential = new CreatedCredentialOffer(
                recordId: response.RecordId,
                protocolState: protocolStateEnum,
                issuerDid: response.IssuingDID,
                subjectDid: subjectDid.Did,
                claims: response.Claims,
                automaticIssuance: response.AutomaticIssuance,
                //schemaId: response.SchemaId,
                validityPeriod: (decimal?)response.ValidityPeriod,
                createdAt: response.CreatedAt,
                jwtCredential: Base64ToJwtDecoder(response.JwtCredential),
                savedLocally: false);
            return Result.Ok(createdCredential);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    private string Base64ToJwtDecoder(string? base64String)
    {
        if (string.IsNullOrEmpty(base64String))
        {
            return string.Empty;
        }

        var output = base64String;
        output = output.Replace('-', '+'); // 62nd char of encoding
        output = output.Replace('_', '/'); // 63rd char of encoding
        switch (output.Length % 4) // Pad with trailing '='s
        {
            case 0: break; // No pad chars in this case
            case 2:
                output += "==";
                break; // Two pad chars
            case 3:
                output += "=";
                break; // One pad char
            default: throw new System.ArgumentOutOfRangeException(nameof(base64String), "Illegal base64url string!");
        }

        var converted = Convert.FromBase64String(output); // Standard base64 decoder
        return System.Text.Encoding.UTF8.GetString(converted);
    }
}