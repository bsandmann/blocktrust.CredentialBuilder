namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;

public interface IAgentService

{
    Agents? AgentsInMemory { get; set; }
    Result<Agent> CreateInMemory(string agentInstanceName, string agentApiKey, string? agentName = "");
    Task GetAgents();
    Task<Result<Agent?>> GetAgent(Guid agentId);

    Task<Result> Save(Agent agent);
    Task<Result> DeleteAgent(Agent agent);
    Task<Result<string>> GetVersion(Agent agent);
}