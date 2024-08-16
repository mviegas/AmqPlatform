namespace AmqPlatform.Core;

public static class ConsumerExtensions
{
    public static void AddConsumer<T>(this IServiceCollection serviceCollection, ConsumerOptions consumerOptions)
    {
        serviceCollection.AddSingleton(_ => consumerOptions);

        serviceCollection.AddHostedService<Consumer<T>>(sp => new Consumer<T>(
            connectionFacade: sp.GetRequiredService<ConnectionFacade>(),
            logger: sp.GetRequiredService<ILogger<Consumer<T>>>(),
            consumerOptions));
    }
}