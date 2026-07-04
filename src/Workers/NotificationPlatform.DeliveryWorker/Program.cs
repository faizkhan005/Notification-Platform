using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationPlatform.DeliveryWorker;
using NotificationPlatform.DeliveryWorker.Consumers;
using NotificationPlatform.DeliveryWorker.Providers;
using Notifications.Application;
using Notifications.Domain;
using Notifications.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog();

// Database access — same NotificationsDbContext as the API, pointing at
// the same Postgres database. The Worker updates notification status
// after delivery attempts.
builder.Services.AddDbContext<NotificationsDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("NotificationsDb"));
});

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationsUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<MailKitEmailSender>();
builder.Services.AddSingleton<IEmailSender>(sp =>
    new ResilientEmailSender(sp.GetRequiredService<MailKitEmailSender>(), sp.GetRequiredService<ILogger<ResilientEmailSender>>()));
builder.Services.AddScoped<CurrentMessageContext>();
builder.Services.AddScoped<ICorrelationIdProvider,ConsumeContextCorrelationIdProvider>();

// MassTransit — CONSUMING side. AddConsumer registers the consumer class;
// UsingRabbitMq configures the transport and tells MassTransit to
// automatically create the queue and bind it to the exchange.
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NotificationCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        // ConfigureEndpoints automatically creates a queue named after the
        // consumer and binds it to the message type's exchange.
        cfg.ConfigureEndpoints(context);

        // Message-level retry: if the consumer throws, MassTransit redelivers
        // the message up to 3 times with delay between attempts. This is
        // SEPARATE from Polly's retry inside ResilientEmailSender — this operates
        // at the message/queue level, Polly operates at the individual network call level.
        // Together: Polly handles "the SMTP call failed, try again quickly,"
        // MassTransit handles "the whole consume operation failed, redeliver
        // the message later and try the entire thing again."
        cfg.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(30)));
    });
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Notification).Assembly));

var host = builder.Build();
host.Run();