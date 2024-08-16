using Microsoft.AspNetCore.Mvc;
using AmqPlatform.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(_ => new ConnectionFacade());
builder.Services.AddConsumer<string>(new ConsumerOptions("test", "/exchange/test/", 10));
builder.Services.AddPublisher(new PublisherOptions<string>("test", "/exchange/test/"));

var app = builder.Build();

app.MapGet("/", async (
    [FromServices] Publisher<string> publisher, 
    [FromQuery] string message) =>
{
    if (string.IsNullOrEmpty(message)) return Results.BadRequest();

    await publisher.PublishAsync(message);

    return Results.Accepted();
});

app.Run();
