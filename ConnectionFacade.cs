using Amqp;
using Amqp.Framing;

namespace Rabbit.ASB.AMQP.PoC;

public class ConnectionFacade
{
    private Connection? _connection;

    public const string Entity = "Test";

    public const string KeyValue = "guest";

    public const string KeyName = "guest";

    public const string Namespace = "0.0.0.0";

    public async Task<Connection> GetConnection()
    {
        if (_connection is not null && !_connection.IsClosed) return _connection;

        _connection =
            await Connection.Factory.CreateAsync(new Address(Namespace, 5672, KeyName, KeyValue, scheme: "AMQP"));

        return _connection;
    }
}