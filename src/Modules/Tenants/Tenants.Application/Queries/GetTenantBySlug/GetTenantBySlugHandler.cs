using BuildingBlocks.Application;
using Tenants.Domain;

namespace Tenants.Application.Queries.GetTenantBySlug;

public sealed class GetTenantBySlugHandler : IQueryHandler<GetTenantBySlugQuery, TenantResponse?>
{
    private readonly ITenantRepository _repository;

    public GetTenantBySlugHandler(ITenantRepository repository)
    {
        _repository = repository;
    }

    public async Task<TenantResponse?> Handle(
        GetTenantBySlugQuery query,
        CancellationToken cancellationToken)
    {
        var slug = TenantSlug.Create(query.Slug);
        var tenant = await _repository.GetBySlugAsync(slug, cancellationToken);

        if (tenant is null) return null;

        return new TenantResponse(
            tenant.Id.Value,
            tenant.Name.Value,
            tenant.Slug.Value,
            tenant.Plan.ToString(),
            tenant.Status.ToString(),
            tenant.CreatedAt,
            tenant.UpdatedAt);
    }
}
