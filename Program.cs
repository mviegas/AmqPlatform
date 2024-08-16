using Microsoft.AspNetCore.Mvc;
using Rabbit.ASB.AMQP.PoC;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(_ => new ConnectionFacade());
builder.Services.AddConsumer(new ConsumerOptions<string>("test", "/exchange/test/"));
builder.Services.AddPublisher(new PublisherOptions<string>("test", "/exchange/test/", "rk"));

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
