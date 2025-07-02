# NotificationServicePublisher

> ⚠️ **ALERT:** This is a test service intended for development and demonstration purposes .

## Overview

NotificationServicePublisher is a service that handles sending notifications via a message broker using MassTransit and RabbitMQ. This service provides a test solution for publishing notification events that can be consumed by other services.

## What is MassTransit?

MassTransit is an open-source distributed application framework for .NET that makes it easier to create applications that use message-based architecture. It provides:

- Abstraction over message brokers (like RabbitMQ, Azure Service Bus, etc.)
- Support for various messaging patterns (publish/subscribe, request/response)
- Easy integration with dependency injection frameworks
- Built-in support for message serialization
- Robust error handling and retries

## RabbitMQ Configuration

This service uses RabbitMQ as the message broker with MassTransit. The configuration is set up as follows:

```csharp
services.AddMassTransit(config =>
{
  config.UsingRabbitMq((context, cfg) =>
  {
    cfg.Host(Configuration["RabbitMQ:Host"], "/", h =>
    {
      h.Username(Configuration["RabbitMQ:Username"]);
      h.Password(Configuration["RabbitMQ:Password"]);
    });
    
    cfg.ConfigureEndpoints(context);
  });
});
```

The RabbitMQ connection settings can be configured in the `appsettings.json` file:

```json
{
  "RabbitMQ": {
  "Host": "rabbitmq://localhost",
  "Username": "guest",
  "Password": "guest",
    "VirtualHost": "jdkwwpno"

  }
}
```

## Notification Message Schema

The notification messages follow a specific schema:

```csharp
    public class NotificationMessage
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public List<ChannelType> Channels { get; set; } = new();
        public List<string>? TargetUsers { get; set; }


        // to identify the notification category (Update, Offer, Alert)
        public NotificationCategory Category { get; set; }

        // NEW: For users not in the system
        public List<string>? ExternalEmails { get; set; }
        public List<string>? ExternalPhoneNumbers { get; set; }
    }

    public enum NotificationType
    {
        SystemWide,
        UserSpecific,
        Group
    }

    public enum NotificationCategory
    {
        Update,  // For updates to existing information
        Offer,   // For promotional offers or discounts
        Alert    // For urgent alerts or notifications
    }

    public enum ChannelType
    {
        Email,
        Push,
        SMS,
        Whatsapp,
        // InApp
    }
```
## API Usage

The service provides a simple endpoint for publishing notifications:

```http
POST /publish
```

This endpoint allows you to publish notification messages to RabbitMQ. Here's how it works behind the scenes:

### Publishing Events with Routing Keys

When publishing events, we use `SetRoutingKey()` to specify the routing patterns that determine how messages are distributed to queues in RabbitMQ:

```csharp
await bus.Publish(evt, ctx =>
{
  ctx.SetRoutingKey("user.notification.created");
});
```

> **Note:** The routing key `user.notification.created` follows a hierarchical naming pattern that helps consumers filter for specific event types. This enables services to subscribe only to the notification events they care about rather than processing all messages. In a production environment, you might use routing keys like `user.notification.{action}` or `{entity}.{action}.{result}` to create a flexible topic-based routing system.

```csharp
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
```

The endpoint uses MassTransit's `IPublishEndpoint` to publish a `NotificationMessage` to the configured RabbitMQ instance with the routing key `user.notification.created`.

## Getting Started

1. Ensure RabbitMQ is installed and running
2. Update `appsettings.json` with your RabbitMQ credentials if needed
3. Run the application
4. Use the API endpoints to publish notifications

## Dependencies

- .NET 8.0+
- MassTransit
- MassTransit.RabbitMQ
- Swashbuckle.AspNetCore (for API documentation)

## Deployment

For production environments, ensure you set up the appropriate RabbitMQ credentials and consider using a secured RabbitMQ instance.
