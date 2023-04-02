namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using Models.Connections;
using PrismAgentApi.Client;
using PrismAgentApi.Model;
using System.Threading;

public class ConnectionService : IConnectionService
{
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
            var createConnectionResponse = await connectionsManagementApi.CreateConnectionAsync(new CreateConnectionRequest(label));
            var connectionId = createConnectionResponse.ConnectionId;
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

    public async Task<bool> WaitOobInvitationResponse(Agent agent, Guid invitationId)
    {
        int attempts = 0;
        Timer timer = null;
        var MaxAttempts = 4;
        const int Interval = 10000;
        var tcs = new TaskCompletionSource<bool>();
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
                //TODO we have to extract the peerd did and maybe all other information from the connection
                //store it in a common model
                
                await timer.DisposeAsync();
                tcs.TrySetResult(true);
            }
            else if (attempts >= MaxAttempts)
            {
                await timer.DisposeAsync();
                tcs.TrySetResult(false);
            }
        }

        void OnTimerElapsed(object state)
        {
            // Call the async local function and ignore the returned task.
            _ = OnTimerElapsedAsync(state);
        }

        timer = new Timer(OnTimerElapsed, null, 0, Interval);

        return await tcs.Task;
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
    
    public async Task<bool> WaitForConnectionConfirmation(Agent agent, Guid invitationId)
    {
        int attempts = 0;
        Timer timer = null;
        var MaxAttempts = 4;
        const int Interval = 10000;
        var tcs = new TaskCompletionSource<bool>();
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
                await timer.DisposeAsync();
                tcs.TrySetResult(true);
            }
            else if (attempts >= MaxAttempts)
            {
                await timer.DisposeAsync();
                tcs.TrySetResult(false);
            }
        }

        void OnTimerElapsed(object state)
        {
            // Call the async local function and ignore the returned task.
            _ = OnTimerElapsedAsync(state);
        }

        timer = new Timer(OnTimerElapsed, null, 0, Interval);

        return await tcs.Task;
    }
}