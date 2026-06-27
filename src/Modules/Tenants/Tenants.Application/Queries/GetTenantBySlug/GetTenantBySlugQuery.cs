using BuildingBlocks.Application;

namespace Tenants.Application.Queries.GetTenantBySlug;

public sealed record GetTenantBySlugQuery(string Slug) : IQuery<TenantResponse?>;
