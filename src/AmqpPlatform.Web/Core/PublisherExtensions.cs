namespace AmqPlatform.Core;

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