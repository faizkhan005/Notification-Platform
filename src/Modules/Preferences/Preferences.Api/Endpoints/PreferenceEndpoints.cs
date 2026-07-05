using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Preferences.Api.Contracts;
using Preferences.Application.Commands.SetPreference;

namespace Preferences.Api.Endpoints;

public static class PreferenceEndpoints
{
    public static IEndpointRouteBuilder MapPreferenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/preferences")
            .WithTags("Preferences");

        group.MapPut("/", SetPreference)
            .WithName("SetPreference")
            .Produces<SetPreferenceResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> SetPreference(
        [FromBody] SetPreferenceRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new SetPreferenceCommand(
            request.TenantId, request.RecipientAddress, request.Channel, request.OptedOut);

        var response = await sender.Send(command, cancellationToken);
        return Results.Ok(response);
    }
}
