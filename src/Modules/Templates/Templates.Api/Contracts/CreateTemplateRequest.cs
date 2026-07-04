namespace Templates.Api.Contracts;

public sealed record CreateTemplateRequest(
    Guid TenantId,
    string Name,
    string Subject,
    string Body
);
