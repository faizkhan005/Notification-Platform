using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Notifications.Api.Contracts;
using Notifications.Application.Commands.SendNotification;
using Notifications.Application.Queries.GetNotificationById;

namespace Notifications.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/notifications")
            .WithTags("Notifications");

        group.MapPost("/", SendNotification)
            .WithName("SendNotification")
            .Produces<SendNotificationResponse>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetNotificationById)
            .WithName("GetNotificationById")
            .Produces<SendNotificationResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> SendNotification(
        [Microsoft.AspNetCore.Mvc.FromBody] SendNotificationRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new SendNotificationCommand(
            request.TenantId,
            request.Channel,
            request.RecipientAddress,
            request.RecipientName,
            request.Subject,
            request.Body);

        var response = await sender.Send(command, cancellationToken);

        // 202 Accepted — we received it, delivery happens asynchronously
        return Results.AcceptedAtRoute("GetNotificationById",
            new { id = response.NotificationId }, response);
    }

    private static async Task<IResult> GetNotificationById(
     Guid id,
     ISender sender,
     CancellationToken cancellationToken)
    {
        var query = new GetNotificationByIdQuery(id);
        var response = await sender.Send(query, cancellationToken);

        return response is null
            ? Results.NotFound(new ProblemDetails
            {
                Title = "Notification not found",
                Detail = $"No notification with id '{id}' exists.",
                Status = StatusCodes.Status404NotFound
            })
            : Results.Ok(response);
    }
}
