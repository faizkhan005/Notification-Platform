namespace Templates.Application.Commands.CreateTemplate;

public sealed record CreateTemplateResponse(
    Guid Id,
    string Name,
    string Subject,
    string Body,
    IReadOnlyList<string> RequiredVariables,
    DateTimeOffset CreatedAt
);