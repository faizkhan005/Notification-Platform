using Microsoft.EntityFrameworkCore;
using NotificationPlatform.Api.Infrastructure;
using Notifications.Api.Endpoints;
using Notifications.Application;
using Notifications.Infrastructure;
using Scalar.AspNetCore;
using Tenants.Api;
using Tenants.Application;
using Tenants.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//tenats
builder.Services.AddTenantsApplication();
builder.Services.AddTenantsInfrastructure(builder.Configuration);

//Notification
builder.Services.AddNotificationsApplication();
builder.Services.AddNotificationsInfrastructure(builder.Configuration);

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
app.MapNotificationsApi();
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