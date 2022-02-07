using System.Text.Json;

namespace Nuages.Identity.UI.Tests;

public static  class JsonSerializerExtensions
{
    // ReSharper disable once UnusedParameter.Global
    public static T? DeserializeAnonymousType<T>(string json, T _, JsonSerializerOptions? options = default)
        => JsonSerializer.Deserialize<T>(json, options);

    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once UnusedParameter.Global
    public static ValueTask<TValue?> DeserializeAnonymousTypeAsync<TValue>(Stream stream, TValue anonymousTypeObject, JsonSerializerOptions? options = default, CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken); // Method to deserialize from a stream added for completeness
}