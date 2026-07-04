using System.Text.RegularExpressions;
using Templates.Domain.Exceptions;

namespace Templates.Domain;

/// <summary>
/// Represents a template's subject and body, including the logic to render
/// them with variable substitution.
///
/// Placeholder syntax: {{variableName}} — double curly braces, matching the
/// convention used by most templating systems (Handlebars, Mustache) so it's
/// immediately familiar to anyone touching this code.
///
/// WHY does rendering logic live in the Domain and not Application?
/// "Render this template with these variables" is a business rule with its
/// own edge cases (missing variable → what happens?), not orchestration.
/// This is exactly the kind of behavior that belongs on a rich domain model
/// rather than being anemic data with logic scattered in a handler.
/// </summary>
public sealed record TemplateContent
{
    private static readonly Regex PlaceholderPattern = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    public string Subject { get; }
    public string Body { get; }

    private TemplateContent(string subject, string body)
    {
        Subject = subject;
        Body = body;
    }

    public static TemplateContent Create(string? subject, string? body)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new TemplateDomainException("Template subject cannot be empty.");

        if (subject.Length > 500)
            throw new TemplateDomainException("Template subject cannot exceed 500 characters.");

        if (string.IsNullOrWhiteSpace(body))
            throw new TemplateDomainException("Template body cannot be empty.");

        return new TemplateContent(subject.Trim(), body.Trim());
    }

    /// <summary>
    /// Renders the subject and body by substituting {{variableName}} placeholders
    /// with values from the provided dictionary.
    ///
    /// WHY throw on a missing variable rather than leaving the placeholder as-is?
    /// A rendered email with a literal "{{name}}" visible to the recipient is a
    /// production bug that should have been caught before sending, not silently
    /// shipped. Failing loudly here surfaces the mistake immediately.
    /// </summary>
    public (string Subject, string Body) Render(IReadOnlyDictionary<string, string> variables)
    {
        var renderedSubject = RenderText(Subject, variables);
        var renderedBody = RenderText(Body, variables);
        return (renderedSubject, renderedBody);
    }

    private static string RenderText(string text, IReadOnlyDictionary<string, string> variables)
    {
        return PlaceholderPattern.Replace(text, match =>
        {
            var variableName = match.Groups[1].Value;

            if (!variables.TryGetValue(variableName, out var value))
                throw new TemplateDomainException(
                    $"Missing value for template variable '{{{{{variableName}}}}}'.");

            return value;
        });
    }

    /// <summary>
    /// Returns the set of variable names referenced in this template, useful
    /// for validating a send request has all required variables before attempting render.
    /// </summary>
    public IReadOnlySet<string> GetRequiredVariables()
    {
        var subjectVars = PlaceholderPattern.Matches(Subject).Select(m => m.Groups[1].Value);
        var bodyVars = PlaceholderPattern.Matches(Body).Select(m => m.Groups[1].Value);
        return subjectVars.Concat(bodyVars).ToHashSet();
    }
}
