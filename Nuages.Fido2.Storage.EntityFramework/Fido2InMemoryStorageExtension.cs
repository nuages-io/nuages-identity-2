using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nuages.Fido2.Storage.EntityFramework;

public static class Fido2InMemoryStorageExtension
{
    public static IFido2Builder AddFido2InMemoryStorage(this IFido2Builder builder, string databaseName )
    {
        builder.Services.AddScoped<IFido2Storage, Fido2StorageEntityFramework>();
        
        builder.Services.AddDbContext<InMemoryFido2DbContext>(options =>
        {
            options.UseInMemoryDatabase(databaseName);
        });

        return builder;
    }
    
}