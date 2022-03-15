using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nuages.Fido2.Storage.EntityFramework;

namespace Nuages.Fido2.Storage.EntifyFramework.MySql;

public static class Fido2MySqlStorageExtension
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IFido2Builder AddFido2MySqlStorage(this IFido2Builder builder, string connectionString )
    {
        builder.Services.AddScoped<IFido2Storage, Fido2StorageEntityFramework<IdentityFido2DbContext>>();
        
        builder.Services.AddDbContext<IdentityFido2DbContext>(options =>
        {
            options
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        });

        return builder;
    }
    
}