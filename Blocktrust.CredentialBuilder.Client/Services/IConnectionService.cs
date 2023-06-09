﻿namespace Blocktrust.CredentialBuilder.Client.Services;

using FluentResults;
using Models;
using Models.Connections;
using PrismAgentApi.Model;

public interface IConnectionService
{
    Task<Result<List<Connection>>> GetListOfConnections(Agent agent);
    Task<Result<OobInvitation>> CreateOobInvitation(Agent agent, string? label = null);
    Task<Result<AcceptedInvitation>> AcceptOobInvitation(Agent agent, string oobInvitation);
    Task<Result<EstablishedConnection>> WaitOobInvitationResponse(Agent agent, Guid invitationId, CancellationToken cancellationToken);
    Task<Result<EstablishedConnection>> WaitForConnectionConfirmation(Agent agent, Guid invitationId, CancellationToken cancellationToken);
}