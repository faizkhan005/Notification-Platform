namespace Tenants.Application.Commands.CreateTenant;

public sealed record CreateTenantResponse(
    Guid Id,
    string Name,
    string Slug,
    string Plan,
    DateTimeOffset CreatedAt
);
