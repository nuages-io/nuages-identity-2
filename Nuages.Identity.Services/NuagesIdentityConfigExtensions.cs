
using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services;

public static class NuagesIdentityConfigExtensions
{
    public static void AddNuagesIdentity(this IdentityBuilder builder)
    {
        builder.Services.AddScoped(typeof(NuagesUserManager<>).MakeGenericType(builder.UserType));
        builder.Services.AddScoped(typeof(NuagesSignInManager<>).MakeGenericType(builder.UserType));

        builder.Services.AddScoped<ILookupProtector, LookupProtector>();
        builder.Services.AddScoped<ILookupProtectorKeyRing, LookupProtectorKeyRing>();
    }
}