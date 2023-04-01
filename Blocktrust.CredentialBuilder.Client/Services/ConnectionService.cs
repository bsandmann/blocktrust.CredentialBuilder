namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using PrismAgentApi.Client;
using PrismAgentApi.Model;

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
}