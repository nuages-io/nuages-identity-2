using Microsoft.Extensions.DependencyInjection;
using Nuages.Identity.Services.Fido2;
using Nuages.Identity.Services.Fido2.Storage;

namespace Nuages.Fido2.Storage.Mongo;

public static class Fido2MongoExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IFido2Builder AddFido2MongoStorage(this IFido2Builder builder, Action<Fido2MongoOptions>? options )
    {
        if (options != null) 
            builder.Services.Configure(options);

        builder.Services.AddScoped<IFido2Storage, MongoFido2Storage>();

        builder.Services.AddHostedService<MongoSchemaInitializer>();
        
        return builder;
    }
}