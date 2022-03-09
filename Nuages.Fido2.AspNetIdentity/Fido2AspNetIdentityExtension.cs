using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Nuages.Fido2.Storage;

namespace Nuages.Fido2.AspNetIdentity;

public static class Fido2AspNetIdentityExtension
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IFido2Builder AddAspNetIdentity<TUser, TKey>(this IFido2Builder builder) 
        where TKey : IEquatable<TKey> 
        where TUser : IdentityUser<TKey>
    {
        builder.Services.AddScoped<IFido2UserStore, Fido2UserStore<TUser, TKey>>();
        builder.Services.AddScoped<IFido2SignInManager, Fido2SignInManager<TUser, TKey>>();
        
        return builder;
    }
}