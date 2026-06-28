using BuildingBlocks.Application;

namespace Tenants.Application.Commands.CreateTenant;

public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    string Plan
) : ICommand<CreateTenantResponse>;
