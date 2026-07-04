using BuildingBlocks.Application;

namespace Templates.Application.Commands.CreateTemplate;

public sealed record CreateTemplateCommand(
    Guid TenantId,
    string Name,
    string Subject,
    string Body
) : ICommand<CreateTemplateResponse>;
