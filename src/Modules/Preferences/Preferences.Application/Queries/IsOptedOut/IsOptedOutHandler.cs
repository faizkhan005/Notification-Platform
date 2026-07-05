using BuildingBlocks.Application;
using Preferences.Domain;
using Tenants.Domain;

namespace Preferences.Application.Queries.IsOptedOut;

public sealed class IsOptedOutHandler : IQueryHandler<IsOptedOutQuery, bool>
{
    private readonly IPreferenceRepository _repository;

    public IsOptedOutHandler(IPreferenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(IsOptedOutQuery query, CancellationToken cancellationToken)
    {
        var tenantId = TenantId.From(query.TenantId);
        var channel = Enum.Parse<Channel>(query.Channel, ignoreCase: true);

        var preference = await _repository.GetAsync(
            tenantId, query.RecipientAddress, channel, cancellationToken);

        // No record → default is opted in → not opted out.
        return preference?.IsOptedOut ?? false;
    }
}
