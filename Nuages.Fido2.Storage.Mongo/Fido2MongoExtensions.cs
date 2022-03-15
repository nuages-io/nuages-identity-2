using Microsoft.Extensions.DependencyInjection;

namespace Nuages.Fido2.Storage.Mongo;

public static class Fido2MongoExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IFido2Builder AddFido2MongoStorage(this IFido2Builder builder, Action<Fido2MongoOptions>? options )
    {
        builder.Services.Configure(options);

        builder.Services.AddScoped<IFido2Storage, MongoFido2Storage>();

        builder.Services.AddHostedService<MongoSchemaInitializer>();
        
        return builder;
    }
}