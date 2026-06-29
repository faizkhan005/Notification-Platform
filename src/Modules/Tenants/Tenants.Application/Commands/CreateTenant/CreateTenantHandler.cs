using BuildingBlocks.Application;
using Tenants.Domain;
using Tenants.Domain.Exceptions;

namespace Tenants.Application.Commands.CreateTenant;

public sealed class CreateTenantHandler
    : ICommandHandler<CreateTenantCommand, CreateTenantResponse>
{
    private readonly ITenantRepository _repository;
    private readonly ITenantsUnitOfWork _unitOfWork;

    public CreateTenantHandler(ITenantRepository repository, ITenantsUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateTenantResponse> Handle(
        CreateTenantCommand command,
        CancellationToken cancellationToken)
    {
        var plan = Enum.Parse<Domain.Plan>(command.Plan, ignoreCase: true);
        var name = TenantName.Create(command.Name);
        var slug = TenantSlug.Create(command.Slug);

        if (await _repository.ExistsBySlugAsync(slug, cancellationToken))
            throw new TenantDomainException($"Slug '{slug}' is already taken.");

        var tenant = Tenant.Create(name, slug, plan);

        await _repository.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTenantResponse(
            tenant.Id.Value,
            tenant.Name.Value,
            tenant.Slug.Value,
            tenant.Plan.ToString(),
            tenant.CreatedAt);
    }
}
