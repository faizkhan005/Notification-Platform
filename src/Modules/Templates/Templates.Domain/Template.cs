using Tenants.Domain;

namespace Templates.Domain;

public sealed class Template
{
    public TemplateId Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public TemplateName Name { get; private set; }
    public TemplateContent Content { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Template()
    {
        Name = null!;
        Content = null!;
    }

    public static Template Create(TenantId tenantId, TemplateName name, TemplateContent content)
    {
        var now = DateTimeOffset.UtcNow;

        return new Template
        {
            Id = TemplateId.New(),
            TenantId = tenantId,
            Name = name,
            Content = content,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Updates the template's content. Templates are mutable — unlike
    /// notifications (which are immutable records of a business event),
    /// a template is a reusable configuration object that tenants edit
    /// over time (fixing a typo, adjusting wording).
    /// </summary>
    public void UpdateContent(TemplateContent newContent)
    {
        Content = newContent;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Renders this template with the given variables, delegating to
    /// TemplateContent's rendering logic.
    /// </summary>
    public (string Subject, string Body) Render(IReadOnlyDictionary<string, string> variables)
        => Content.Render(variables);
}
