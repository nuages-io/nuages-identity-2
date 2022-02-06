
using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Stores.Mongo;


public static class AspNetIdentityMongoExtensions
{
    public static void AddMongoStores<TUser, TRole, TKey>(this IdentityBuilder builder, Action<MongoIdentityOptions> configure)  
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        builder.Services.Configure(configure);

        

        builder.AddUserStore<MongoUserStore<TUser, TRole, TKey>>();
        builder.AddRoleStore<MongoRoleStore<TRole, TKey>>();
        
        builder.Services.AddHostedService<MongoSchemaInitializer<TUser, TRole, TKey>>();
    }
}