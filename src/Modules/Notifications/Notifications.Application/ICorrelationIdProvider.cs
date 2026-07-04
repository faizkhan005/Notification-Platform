namespace Notifications.Application;

/// <summary>
/// Abstracts "what is the current correlation ID" so Infrastructure code
/// (like UnitOfWork) never needs to know whether it's running inside an
/// HTTP request (API) or a message consumer (Worker).
///
/// The API's implementation reads from IHttpContextAccessor.
/// The Worker has no HTTP context at all, so it provides a no-op
/// implementation (or, in a more advanced setup, one that reads from
/// the current MassTransit ConsumeContext instead).
/// </summary>
public interface ICorrelationIdProvider
{
    string? GetCorrelationId();
}
