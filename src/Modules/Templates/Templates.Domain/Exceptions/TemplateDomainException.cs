namespace Templates.Domain.Exceptions;

public sealed class TemplateDomainException : Exception
{
    public TemplateDomainException(string message) : base(message) { }
}