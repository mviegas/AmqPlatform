using System.Text;
using Amqp;
using Amqp.Framing;
using Rabbit.ASB.AMQP.PoC.Exceptions;

namespace Rabbit.ASB.AMQP.PoC;

public record PublisherOptions<T>(string Name, string Address, string Subject);

public static class PublisherExtensions
{
    public static void AddPublisher<T>(this IServiceCollection serviceCollection, PublisherOptions<T> publisherOptions)
    {
        serviceCollection.AddSingleton(publisherOptions);

        serviceCollection.AddScoped<Publisher<T>>(sp => new Publisher<T>(
            connectionFacade: sp.GetRequiredService<ConnectionFacade>(),
            publisherOptions));
    }
}

public class Publisher<T>(ConnectionFacade connectionFacade, PublisherOptions<T> publisherOptions)
{
    public async Task PublishAsync(string messageContent)
    {
        try
        {
            var connection = await connectionFacade.GetConnection();

            var session = new Session(connection);
            
            SenderLink sender = new SenderLink(session, publisherOptions.Name, publisherOptions.Address);

            var message = new Message();

            message.Properties = new Properties();
            message.Properties.SetMessageId(Guid.NewGuid());
            message.Properties.Subject = publisherOptions.Subject;
            message.BodySection = new Data { Binary = Encoding.UTF8.GetBytes(messageContent) };

            await sender.SendAsync(message);

            await sender.CloseAsync();
        }
        catch (AmqpException e) when (e.Error.Condition == ErrorCode.MessageReleased)
        {
            throw new SenderWithoutBindingException(publisherOptions.Subject, e);
        }
    }
}