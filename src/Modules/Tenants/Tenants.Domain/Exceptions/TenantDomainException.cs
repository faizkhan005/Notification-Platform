namespace Tenants.Domain.Exceptions;

public sealed class TenantDomainException : Exception
{
    public TenantDomainException(string message) : base(message) { }
    public TenantDomainException(string message, Exception inner) : base(message, inner) { }
}
