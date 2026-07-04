using Templates.Domain;

namespace Templates.Infrastructure.Persistence;

public sealed class TemplateRepository : ITemplateRepository
{
    private readonly TemplatesDbContext _context;

    public TemplateRepository(TemplatesDbContext context)
    {
        _context = context;
    }

    public async Task<Template?> GetByIdAsync(TemplateId id, CancellationToken cancellationToken = default)
        => await _context.Templates.FindAsync([id], cancellationToken);

    public async Task AddAsync(Template template, CancellationToken cancellationToken = default)
        => await _context.Templates.AddAsync(template, cancellationToken);
}