using BuildingBlocks.Application;

namespace Templates.Application.Queries.GetTemplateById;

public sealed record GetTemplateByIdQuery(Guid Id) : IQuery<TemplateResponse?>;
