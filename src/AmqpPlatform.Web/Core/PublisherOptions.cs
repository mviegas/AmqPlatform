namespace AmqPlatform.Core;

public record PublisherOptions<T>(string Name, string Address, string? Subject = null);