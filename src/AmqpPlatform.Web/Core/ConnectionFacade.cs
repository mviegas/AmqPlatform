using Amqp;

namespace AmqPlatform.Core;

public sealed class ConnectionFacade
{
    // TODO: Remove to setup in init
    public ConnectionFacade()
    {
    }

    public ConnectionFacade(string host, string entity, string user, string pass)
    {
        Host = host;
        Entity = entity;
        User = user;
        Pass = pass;
    }

    private Connection? _connection;

    public string Host { get; init; } = "0.0.0.0";
    public string Entity { get; init; } = "Test";
    public string User { get; init; } = "guest";
    public string Pass { get; init; } = "guest";

    public async Task<Result<Connection?>> TryGetConnection()
    {
        try
        {
            if (_connection is not null && !_connection.IsClosed) return new Result<Connection?>(true, _connection);

            _connection =
                await Connection.Factory.CreateAsync(new Address(Host, 5672, Pass, User, scheme: "AMQP"));

            return new Result<Connection?>(true, _connection, null);
        }
        catch (Exception e)
        {
            return new Result<Connection?>(false, null, e);
        }
    }

    public record Result<T>(bool Success, T? Object, Exception? Error = null);
}