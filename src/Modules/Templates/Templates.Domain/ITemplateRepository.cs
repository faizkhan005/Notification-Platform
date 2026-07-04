namespace Templates.Domain;

public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(TemplateId id, CancellationToken cancellationToken = default);
    Task AddAsync(Template template, CancellationToken cancellationToken = default);
}