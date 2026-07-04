using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationPlatform.DeliveryWorker.Consumers;
using NotificationPlatform.DeliveryWorker.Providers;
using Notifications.Application;
using Notifications.Domain;
using Notifications.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

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
    });
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Notification).Assembly));

var host = builder.Build();
host.Run();