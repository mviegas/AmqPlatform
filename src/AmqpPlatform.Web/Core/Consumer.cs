using System.Text;
using Amqp;

namespace AmqPlatform.Core;

public class Consumer<T>(
    ConnectionFacade connectionFacade,
    ILogger<Consumer<T>> logger,
    ConsumerOptions consumerOptions)
    : IHostedService
{
    private Session? _session;
    private ReceiverLink? _receiver;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = await connectionFacade.TryGetConnection();
        if (connection.Success)
        {
            _session = new Session(connection.Object);
            _receiver = new ReceiverLink(_session, consumerOptions.Name, consumerOptions.Address);
            _receiver.Start(1, (_, message) =>
            {
                if (message.Body is byte[] bodyBytes)
                {
                    logger.LogInformation("Message Received: {Message} at {Timestamp}",
                        Encoding.UTF8.GetString(bodyBytes.AsSpan()), DateTime.UtcNow);
                    _receiver.Accept(message);
                }
                else
                {
                    logger.LogError("Message could not be deserialized");
                    _receiver.Reject(message);
                }
            });
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_receiver is not null) await _receiver.CloseAsync();
        if (_session is not null) await _session.CloseAsync();
    }
}