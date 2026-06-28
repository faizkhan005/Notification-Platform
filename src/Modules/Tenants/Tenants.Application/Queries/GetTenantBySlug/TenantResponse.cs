namespace Tenants.Application.Queries.GetTenantBySlug;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Slug,
    string Plan,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
