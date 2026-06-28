using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Tenants.Api.Contracts;
using Tenants.Application.Commands.CreateTenant;
using Tenants.Application.Queries.GetTenantBySlug;

namespace Tenants.Api.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/tenants")
            .WithTags("Tenants");

        group.MapPost("/", CreateTenant)
            .WithName("CreateTenant")
            .Produces<CreateTenantResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/{slug}", GetTenantBySlug)
            .WithName("GetTenantBySlug")
            .Produces<TenantResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CreateTenant(
        [FromBody] CreateTenantRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateTenantCommand(request.Name, request.Slug, request.Plan);
        var response = await sender.Send(command, cancellationToken);

        return Results.CreatedAtRoute("GetTenantBySlug", new { slug = response.Slug }, response);
    }

    private static async Task<IResult> GetTenantBySlug(
        string slug,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetTenantBySlugQuery(slug);
        var response = await sender.Send(query, cancellationToken);

        return response is null
            ? Results.NotFound(new ProblemDetails
            {
                Title = "Tenant not found",
                Detail = $"No tenant with slug '{slug}' exists.",
                Status = StatusCodes.Status404NotFound
            })
            : Results.Ok(response);
    }
}