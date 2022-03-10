using Fido2NetLib;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Nuages.Fido2.Storage;

namespace Nuages.Fido2.AspNetIdentity;

public static class Fido2AspNetIdentityExtension
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    // public static IFido2Builder AddAspNetIdentity<TUser, TKey>(this IFido2Builder builder) 
    //     where TKey : IEquatable<TKey> 
    //     where TUser : IdentityUser<TKey>
    // {
    //    
    //     return builder;
    // }
    
    public static Fido2Builder AddNuagesFido2<TUser, TKey>(this IdentityBuilder builder,  Action<Fido2Configuration> setupAction)
        where TKey : IEquatable<TKey> 
        where TUser : IdentityUser<TKey>
    {
        var fido2Builder = new Fido2Builder(builder.Services);
        
        var userType = builder.UserType;
        
        //var userStoreType = typeof(PasswordlessLoginProvider<>).MakeGenericType(userType);
        
        fido2Builder.Services.AddScoped<IFido2UserStore, Fido2UserStore<TUser, TKey>>();
        fido2Builder.Services.AddScoped<IFido2SignInManager, Fido2SignInManager<TUser, TKey>>();

        fido2Builder.Services.AddNuagesFido2(setupAction);

        builder.AddTokenProvider<Fifo2UserTwoFactorTokenProvider<TUser, TKey>>("FIDO2");
        
        return fido2Builder;
    }
}