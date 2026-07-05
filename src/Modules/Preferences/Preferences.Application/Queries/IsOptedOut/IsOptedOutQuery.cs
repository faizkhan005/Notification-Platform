using BuildingBlocks.Application;

namespace Preferences.Application.Queries.IsOptedOut;

public sealed record IsOptedOutQuery(
    Guid TenantId,
    string RecipientAddress,
    string Channel
) : IQuery<bool>;