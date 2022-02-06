using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Stores.InMemory;

// ReSharper disable once UnusedType.Global
public static class AspNetIdentityInMemoryExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static void AddInMemoryStores<TUser, TRole, TKey>(this IdentityBuilder builder)  
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        builder.Services.AddScoped<IInMemoryStorage<TRole>, InMemoryStorage<TRole, TKey>>();
        
        builder.AddUserStore<InMemoryUserStore<TUser, TRole, TKey>>();
        builder.AddRoleStore<InMemoryRoleStore<TRole, TKey>>();
    }
}