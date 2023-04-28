namespace Blocktrust.CredentialBuilder.Client.Services;

using System.Net;
using System.Text;
using FluentResults;
using Models;
using Models.Connections;
using PrismAgentApi.Client;
using PrismAgentApi.Model;
using System.Threading;

public class ConnectionService : IConnectionService
{
    public static readonly string UnnamedConnectionLabel = "unnamed connection";
    private readonly HttpClient _httpClient;

    public ConnectionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public async Task<Result<List<Connection>>> GetListOfConnections(Agent agent)
    {
        Blocktrust.PrismAgentApi.Api.ConnectionsManagementApi connectionsManagementApi = new Blocktrust.PrismAgentApi.Api.ConnectionsManagementApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));
        try
        {
            //TODO implement pagination

            var response = await connectionsManagementApi.GetConnectionsAsync();
            var connections = response.Contents;
            return Result.Ok(connections);
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<OobInvitation>> CreateOobInvitation(Agent agent, string? label = null)
    {
        Blocktrust.PrismAgentApi.Api.ConnectionsManagementApi connectionsManagementApi = new Blocktrust.PrismAgentApi.Api.ConnectionsManagementApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));
        try
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                label = UnnamedConnectionLabel;
            }

            var response = await connectionsManagementApi.CreateConnectionAsync(new CreateConnectionRequest(label));
            var invitationUrl = response.Invitation.InvitationUrl;
            var invitationId = response.Invitation.Id;
            var invitationFrom = response.Invitation.From;

            return Result.Ok(new OobInvitation(invitationUrl, invitationId, invitationFrom));
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<EstablishedConnection>> WaitOobInvitationResponse(Agent agent, Guid invitationId, CancellationToken cancellationToken)
    {
        int attempts = 0;
        var tcs = new TaskCompletionSource<Result<EstablishedConnection>>();

        cancellationToken.Register(() => tcs.TrySetCanceled());

        Blocktrust.PrismAgentApi.Api.ConnectionsManagementApi connectionsManagementApi = new Blocktrust.PrismAgentApi.Api.ConnectionsManagementApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));

        async Task OnTimerElapsedAsync(object state)
        {
            attempts++;
            var response = await GetListOfConnections(agent);
            var connection = response.Value.FirstOrDefault(c => c.Invitation.Id.Equals(invitationId));
            if (connection is not null && connection.State == Connection.StateEnum.ConnectionResponseSent)
            {
                var label = connection.Label;
                var localDid = connection.MyDid;
                var remoteDid = connection.TheirDid;
                var connectionId = connection.ConnectionId;
                var establishedConnection = new EstablishedConnection(localDid, remoteDid, invitationId, connectionId, label);
                tcs.TrySetResult(Result.Ok(establishedConnection));
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


    public async Task<Result<AcceptedInvitation>> AcceptOobInvitation(Agent agent, string oobInvitation)
    {
        Blocktrust.PrismAgentApi.Api.ConnectionsManagementApi connectionsManagementApi = new Blocktrust.PrismAgentApi.Api.ConnectionsManagementApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));

        if (oobInvitation.Contains("?_oob="))
        {
            oobInvitation = oobInvitation.Split("?_oob=")[1];
        }
        else if (oobInvitation.Contains("?_oobid="))
        {
            // TODO fix correct way to get redirected URL -> Save workaround as in the Wallet!
            // When the code enters the branch we have a shortenedUrl. The shortenedUrl will get redirected
            // to the original URl with the full OOB string. To get to the redirected URL we need to read
            // the response header "Location" or read the "response.RequestMessage.RequestUri" property.
            // Both methods are not working in Blazor WebAssembly. We would have to call a underlying 
            // JavaScript function to get the redirected URL. This should be implemented in the future.
            // For now we use our mediator-server to get the redirected URL, by calling a temporary endpoint
            // which gets the redirected URL and returns it to the client.

            try
            {
                var url = new Uri(oobInvitation);
                var byteArray = Encoding.UTF8.GetBytes(url.AbsoluteUri);
                var encodedUrl = Convert.ToBase64String(byteArray);
                var redirectUrlRequest = new Uri($"https://mediator.blocktrust.dev/redirectUrl?encodedUrl={encodedUrl}");

                var response = await _httpClient.GetAsync(redirectUrlRequest);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return Result.Fail("Unable to get redirected URL");
                }

                var resultContent = await response.Content.ReadAsStringAsync();
                if (resultContent.Contains("?_oob=", StringComparison.InvariantCultureIgnoreCase))
                {
                    var split = resultContent.Split("?_oob=");
                    if (split.Length != 2)
                    {
                        return Result.Fail("Unable to parse OOB invitation");
                    }

                    oobInvitation = split[1];
                }
            }
            catch (Exception e)
            {
                return Result.Fail("Unable to parse OOB invitation");
            }
        }

        try
        {
            var acceptConnectionResponse = await connectionsManagementApi.AcceptConnectionInvitationAsync(new AcceptConnectionInvitationRequest(oobInvitation));
            return Result.Ok(new AcceptedInvitation(
                acceptConnectionResponse.ConnectionId,
                acceptConnectionResponse.Invitation.Id,
                acceptConnectionResponse.MyDid,
                acceptConnectionResponse.TheirDid,
                acceptConnectionResponse.State,
                acceptConnectionResponse.UpdatedAt
            ));
        }
        catch (Exception e)
        {
            return Result.Fail(e.Message);
        }
    }

    public async Task<Result<EstablishedConnection>> WaitForConnectionConfirmation(Agent agent, Guid invitationId, CancellationToken cancellationToken)
    {
        int attempts = 0;
        var tcs = new TaskCompletionSource<Result<EstablishedConnection>>();

        cancellationToken.Register(() => tcs.TrySetCanceled());

        Blocktrust.PrismAgentApi.Api.ConnectionsManagementApi connectionsManagementApi = new Blocktrust.PrismAgentApi.Api.ConnectionsManagementApi(
            configuration: new Configuration(defaultHeaders: new Dictionary<string, string>() { { "apiKey", agent.AgentApiKey } },
                apiKey: new Dictionary<string, string>() { },
                apiKeyPrefix: new Dictionary<string, string>(),
                basePath: agent.AgentInstanceUri.AbsoluteUri));

        async Task OnTimerElapsedAsync(object state)
        {
            attempts++;
            var response = await GetListOfConnections(agent);
            var connection = response.Value.FirstOrDefault(c => c.Invitation.Id.Equals(invitationId));
            if (connection is not null && connection.State == Connection.StateEnum.ConnectionResponseReceived)
            {
                var label = connection.Label;
                var localDid = connection.MyDid;
                var remoteDid = connection.TheirDid;
                var connectionId = connection.ConnectionId;
                var establishedConnection = new EstablishedConnection(localDid, remoteDid, invitationId, connectionId, label);
                tcs.TrySetResult(Result.Ok(establishedConnection));
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