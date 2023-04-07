namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using Models.Connections;
using PrismAgentApi.Client;
using PrismAgentApi.Model;
using System.Threading;

public class ConnectionService : IConnectionService
{
    public static readonly string UnnamedConnectionLabel = "unnamed connection";
    
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

            var connectionsResponse = await connectionsManagementApi.GetConnectionsAsync();
            var connections = connectionsResponse.Contents;
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
            var createConnectionResponse = await connectionsManagementApi.CreateConnectionAsync(new CreateConnectionRequest(label));
            var invitationUrl = createConnectionResponse.Invitation.InvitationUrl;
            var invitationId = createConnectionResponse.Invitation.Id;
            var invitationFrom = createConnectionResponse.Invitation.From;

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
        var MaxAttempts = 20;
        const int Interval = 5000;
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
            var connections = await GetListOfConnections(agent);
            var connection = connections.Value.FirstOrDefault(c => c.Invitation.Id.Equals(invitationId));
            if (connection is not null && connection.State == Connection.StateEnum.ConnectionResponseSent)
            {
                var label = connection.Label;
                var localDid = connection.MyDid;
                var remoteDid = connection.TheirDid;
                var connectionId = connection.ConnectionId;
                var establishedConnection = new EstablishedConnection(localDid, remoteDid, invitationId, connectionId, label);
                tcs.TrySetResult(Result.Ok(establishedConnection));
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
        var MaxAttempts = 20;
        const int Interval = 5000;
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
            var connections = await GetListOfConnections(agent);
            var connection = connections.Value.FirstOrDefault(c => c.Invitation.Id.Equals(invitationId));
            if (connection is not null && connection.State == Connection.StateEnum.ConnectionResponseReceived)
            {
                var label = connection.Label;
                var localDid = connection.MyDid;
                var remoteDid = connection.TheirDid;
                var connectionId = connection.ConnectionId;
                var establishedConnection = new EstablishedConnection(localDid, remoteDid, invitationId, connectionId, label); 
                tcs.TrySetResult(Result.Ok(establishedConnection));
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
}