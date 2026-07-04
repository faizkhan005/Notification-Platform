using Microsoft.EntityFrameworkCore;
using NotificationPlatform.Api.Infrastructure;
using Notifications.Api;
using Notifications.Application;
using Notifications.Infrastructure;
using Scalar.AspNetCore;
using Tenants.Api;
using Tenants.Application;
using Tenants.Infrastructure;

using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);

    //routing internal logs through serial logs
    builder.Host.UseSerilog();

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

        var notificationsDb = scope.ServiceProvider.GetRequiredService<Notifications.Infrastructure.Persistence.NotificationsDbContext>();
        await notificationsDb.Database.MigrateAsync();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Notification Platform API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;