using System.Text.Json.Serialization;
using MassTransit;
using NotificationService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// builder.Services.AddMassTransit(x =>
// {
//     x.UsingRabbitMq((context, cfg) =>
//     {
//         cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
//         {
//             h.Username(builder.Configuration["RabbitMQ:Username"]);
//             h.Password(builder.Configuration["RabbitMQ:Password"]);
//         });

//         // Configure the message to use a specific exchange
//         cfg.Message<NotificationMessage>(c =>
//         {
//             c.SetEntityName("notifications.exchange");
//         });

//         // Configure publish settings
//         cfg.Publish<NotificationMessage>(c =>
//         {
//             c.ExchangeType = "topic";
//         });

//         cfg.ConfigureEndpoints(context);
//     });
// });

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], builder.Configuration["RabbitMQ:VirtualHost"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        // Configure message type mapping
        cfg.Message<NotificationMessage>(c =>
        {
            c.SetEntityName("NotificationMessage"); // Use simple name
        });

        // Configure for cross-platform compatibility
        cfg.ConfigureJsonSerializerOptions(options =>
        {
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddLogging(configure => configure.AddConsole());





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();




app.MapPost("/publish", async (IPublishEndpoint bus) =>
{
    var evt = new NotificationMessage
    {
        Title = $"Test Notification",
        Body = $"Test Notification Content",
        Type = NotificationType.Group,
        Channels = new List<ChannelType> { ChannelType.Email },
        TargetUsers = new List<string> { "g1623g6-12g31g-123g-123g-123g123g", "g1623g6-12g31g-123g-123g-123g123g" },
        Category = NotificationCategory.Update
    };

    await bus.Publish(evt, ctx =>
    {
        ctx.SetRoutingKey("user.notification.created");
    });

    return Results.Ok("Published");
});

app.Run();

