using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nuages.Fido2.Storage.EntityFramework.SqlServer;

public static class Fido2InMemoryStorageExtension
{
    public static IFido2Builder AddFidoSqlServerStorage(this IFido2Builder builder, string connectionString )
    {
        builder.Services.AddScoped<IFido2Storage, Fido2StorageEntityFramework>();
        
        builder.Services.AddDbContext<IdentityFido2SqlServerDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        return builder;
    }
    
}