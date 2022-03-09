using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Nuages.Fido2.Storage;

namespace Nuages.Fido2.AspNetIdentity;

public static class Fido2AspNetIdentityExtension
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IFido2Builder AddAspNetIdentityStores<TUser, TKey>(this IFido2Builder builder) 
        where TKey : IEquatable<TKey> 
        where TUser : IdentityUser<TKey>
    {
        builder.Services.AddScoped<IFido2UserStore, Fido2UserStore<TUser, TKey>>();

        return builder;
    }
}