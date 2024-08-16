using Amqp;

namespace Rabbit.ASB.AMQP.PoC.Exceptions;

public class SenderWithoutBindingException(string subject, AmqpException innerException)
    : Exception($"Sender without receiver binding for subject {subject}, please check transport.", innerException)
{
    public string Subject { get; } = subject;
}