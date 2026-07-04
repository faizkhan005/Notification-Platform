using BuildingBlocks.Application;
using Templates.Domain;

namespace Templates.Application.Queries.GetTemplateById;

public sealed class GetTemplateByIdHandler : IQueryHandler<GetTemplateByIdQuery, TemplateResponse?>
{
    private readonly ITemplateRepository _repository;

    public GetTemplateByIdHandler(ITemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<TemplateResponse?> Handle(
        GetTemplateByIdQuery query,
        CancellationToken cancellationToken)
    {
        var id = TemplateId.From(query.Id);
        var template = await _repository.GetByIdAsync(id, cancellationToken);

        if (template is null) return null;

        return new TemplateResponse(
            template.Id.Value,
            template.TenantId.Value,
            template.Name.Value,
            template.Content.Subject,
            template.Content.Body,
            template.Content.GetRequiredVariables().ToList(),
            template.CreatedAt,
            template.UpdatedAt);
    }
}

