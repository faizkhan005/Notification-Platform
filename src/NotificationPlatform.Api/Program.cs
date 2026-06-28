using Microsoft.EntityFrameworkCore;
using NotificationPlatform.Api.Infrastructure;
using Scalar.AspNetCore;
using Tenants.Api;
using Tenants.Application;
using Tenants.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTenantsApplication();
builder.Services.AddTenantsInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Notification Platform API";
        options.Theme = ScalarTheme.Purple;
    });
}

app.UseHttpsRedirection();
app.MapTenantsApi();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }))
    .WithTags("Health");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Tenants.Infrastructure.Persistence.TenantsDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();

public partial class Program;