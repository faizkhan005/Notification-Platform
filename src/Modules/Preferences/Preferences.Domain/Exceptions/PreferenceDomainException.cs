namespace Preferences.Domain.Exceptions;

public sealed class PreferenceDomainException : Exception
{
    public PreferenceDomainException(string message) : base(message) { }
}