using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Templates.Api.Contracts;
using Templates.Application.Commands.CreateTemplate;
using Templates.Application.Queries.GetTemplateById;

namespace Templates.Api.Endpoints;

public static class TemplateEndpoints
{
    public static IEndpointRouteBuilder MapTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/templates")
            .WithTags("Templates");

        group.MapPost("/", CreateTemplate)
            .WithName("CreateTemplate")
            .Produces<CreateTemplateResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetTemplateById)
            .WithName("GetTemplateById")
            .Produces<TemplateResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> CreateTemplate(
        [FromBody] CreateTemplateRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateTemplateCommand(request.TenantId, request.Name, request.Subject, request.Body);
        var response = await sender.Send(command, cancellationToken);
        return Results.CreatedAtRoute("GetTemplateById", new { id = response.Id }, response);
    }

    private static async Task<IResult> GetTemplateById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetTemplateByIdQuery(id);
        var response = await sender.Send(query, cancellationToken);

        return response is null
            ? Results.NotFound(new ProblemDetails
            {
                Title = "Template not found",
                Detail = $"No template with id '{id}' exists.",
                Status = StatusCodes.Status404NotFound
            })
            : Results.Ok(response);
    }
}
