using System.Text;
using Amqp;
using Amqp.Framing;
using AmqPlatform.Core.Exceptions;

namespace AmqPlatform.Core;

public class Publisher<T>(ConnectionFacade connectionFacade, PublisherOptions<T> publisherOptions)
{
    public async Task PublishAsync(string messageContent)
    {
        try
        {
            var connection = await connectionFacade.TryGetConnection();

            var session = new Session(connection.Object);
            
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