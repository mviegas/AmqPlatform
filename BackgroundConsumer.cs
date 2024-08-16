using System.Text;
using Amqp;

namespace Rabbit.ASB.AMQP.PoC;

public record ConsumerOptions<T>(string Name, string Address);

public static class ConsumerExtensions
{
    public static void AddConsumer<T>(this IServiceCollection serviceCollection, ConsumerOptions<T> consumerOptions)
    {
        serviceCollection.AddSingleton(_ => consumerOptions);
        
        serviceCollection.AddHostedService<BackgroundConsumer<T>>(sp => new BackgroundConsumer<T>(
            connectionFacade: sp.GetRequiredService<ConnectionFacade>(),
            logger: sp.GetRequiredService<ILogger<BackgroundConsumer<T>>>(),
            consumerOptions));
    }
}

public class BackgroundConsumer<T>(ConnectionFacade connectionFacade, ILogger<BackgroundConsumer<T>> logger, ConsumerOptions<T> consumerOptions)
    : IHostedService
{
    private Session _session;
    private ReceiverLink _receiver;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = await connectionFacade.GetConnection();
        _session = new Session(connection);
        _receiver = new ReceiverLink(_session, consumerOptions.Name, consumerOptions.Address);
        _receiver.Start(1, (_, message) =>
        {
            if (message.Body is byte[] bodyBytes)
            {
                logger.LogInformation("Message Received: {Message} at {Timestamp}", Encoding.UTF8.GetString(bodyBytes.AsSpan()), DateTime.UtcNow);
                _receiver.Accept(message);
            }
            else
            {
                logger.LogError("Message could not be deserialized");
                _receiver.Reject(message);
            }
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _receiver.CloseAsync();
        await _session.CloseAsync();
    }
}