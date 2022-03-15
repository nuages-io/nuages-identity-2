using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nuages.Fido2.Storage.EntityFramework;

public static class Fido2InMemoryStorageExtension
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IFido2Builder AddFido2InMemoryStorage(this IFido2Builder builder, string databaseName )
    {
        builder.Services.AddScoped<IFido2Storage, Fido2StorageEntityFramework<IdentityFido2DbContext>>();
        
        builder.Services.AddDbContext<IdentityFido2DbContext>(options =>
        {
            options.UseInMemoryDatabase(databaseName);
        });

        return builder;
    }
    
}