namespace Tenants.Api.Contracts;

public sealed record CreateTenantRequest(
    string Name,
    string Slug,
    string Plan
);