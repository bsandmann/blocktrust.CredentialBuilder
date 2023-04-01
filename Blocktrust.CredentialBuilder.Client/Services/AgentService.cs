namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;

public class AgentService : IAgentService
{
    public Agents? AgentsInMemory { get; set; }
    public IStorageService StorageService { get; set; }


    public AgentService(IStorageService storageService)
    {
        StorageService = storageService;
    }

    private async Task<Result> LoadAgentsFromStorage()
    {
        var agentsResult = await StorageService.GetItem<Agents>();
        if (agentsResult.IsFailed)
        {
            agentsResult.ToResult();
        }

        if (agentsResult.Value is null)
        {
            AgentsInMemory = new Agents() { List = new List<Agent>() };
        }
        else
        {
            AgentsInMemory = agentsResult.Value;
        }

        return Result.Ok();
    }

    public async Task GetAgents()
    {
        var agentsResult = await StorageService.GetItem<Agents>();
        if (agentsResult.IsFailed || agentsResult.Value == null)
        {
            AgentsInMemory = new Agents();
        }
        else
        {
            AgentsInMemory = agentsResult.Value;
        }
    }

    public async Task<Result<Agent?>> GetAgent(Guid agentId)
    {
        await GetAgents();
        var agent = AgentsInMemory!.List.FirstOrDefault(x => x.AgentId.Equals(agentId));
        return Result.Ok(agent);
    }


    public Result<Agent> CreateInMemory(string agentInstanceName, string agentApiKey, string? agentName = "")
    {
        if (string.IsNullOrEmpty(agentInstanceName) || string.IsNullOrEmpty(agentApiKey))
        {
            return Result.Fail("Invalid agent instance name or agent api key");
        }

        Uri? agentInstanceUri;
        if (agentInstanceName.Contains("localhost", StringComparison.InvariantCultureIgnoreCase) || agentInstanceName.Contains("host.docker.internal", StringComparison.InvariantCultureIgnoreCase))
        {
            var isValidUri = Uri.TryCreate(agentInstanceName, UriKind.Absolute, out agentInstanceUri);
            if (!isValidUri)
            {
                return Result.Fail("Invalid agent instance name. Cannot be converted to a valid URI");
            }
        }
        else
        {
            var isValidUri = Uri.TryCreate($"https://{agentInstanceName}.atalaprism.io/prism-agent", UriKind.Absolute, out agentInstanceUri);
            if (!isValidUri)
            {
                return Result.Fail("Invalid agent instance name. Cannot be converted to a valid URI");
            }
        }

        if (agentInstanceUri is null)
        {
            return Result.Fail("Invalid agent instance name");
        }

        if (string.IsNullOrEmpty(agentName))
        {
            agentName = agentInstanceName;
        }

        return Result.Ok(new Agent(
            agentInstanceUri: agentInstanceUri,
            agentApiKey: agentApiKey,
            agentName: agentName
        ));
    }

    public async Task<Result> Save(Agent agent)
    {
        if (AgentsInMemory is null)
        {
            return Result.Fail("Agents could not be loaded");
        }

        var existingAgent = AgentsInMemory.List.FirstOrDefault(p => p.AgentId.Equals(agent.AgentId));
        if (existingAgent is null)
        {
            AgentsInMemory.List.Add(agent);
        }
        else
        {
            existingAgent = agent;
        }

        var storageResult = await StorageService.SetItem<Agents>(AgentsInMemory);
        if (storageResult.IsFailed)
        {
            return storageResult;
        }

        return Result.Ok();
    }

    public async Task<Result> DeleteAgent(Agent agent)
    {
        if (AgentsInMemory is null)
        {
            return Result.Fail("Agents could not be loaded");
        }

        var existingAgent = AgentsInMemory.List.FirstOrDefault(p => p.AgentId.Equals(agent.AgentId));
        if (existingAgent is null)
        {
            return Result.Ok();
        }

        foreach (var agentInMemory in AgentsInMemory.List)
        {
            if (agentInMemory.AgentId.Equals(agent.AgentId))
            {
                AgentsInMemory.List.Remove(agentInMemory);
                await StorageService.SetItem<Agents>(AgentsInMemory);
                break;
            }
        }

        await GetAgents();
        return Result.Ok();
    }
}