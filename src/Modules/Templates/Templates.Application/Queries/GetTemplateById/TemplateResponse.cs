using System;
using System.Collections.Generic;
using System.Text;

namespace Templates.Application.Queries.GetTemplateById;

public sealed record TemplateResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string Subject,
    string Body,
    IReadOnlyList<string> RequiredVariables,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
