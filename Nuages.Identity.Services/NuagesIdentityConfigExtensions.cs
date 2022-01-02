
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nuages.Identity.Services;

public static class NuagesIdentityConfigExtensions
{
    public static void AddNuagesIdentity(this IdentityBuilder builder)
    {

        builder.Services.AddScoped(typeof(NuagesUserManager<>).MakeGenericType(builder.UserType));
        builder.Services.AddScoped(typeof(NuagesSignInManager<>).MakeGenericType(builder.UserType));

        builder.Services.AddScoped<ILookupProtector, LookupProtector>();
        builder.Services.AddScoped<ILookupProtectorKeyRing, LookupProtectorKeyRing>();
        
        //builder.Services.TryAddScoped<NuagesUserManager<TUser>>();
        // builder.Services.TryAddScoped<NuagesSignInManager<TUser>>();
        //services.TryAddScoped<RoleManager<TRole>>();
    }
}