
using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services;

public static class NuagesIdentityConfigExtensions
{
    public static void AddNuagesIdentity(this IdentityBuilder builder,
        Action<NuagesIdentityOptions> configure )
    {
        AddNuagesIdentity(builder, null, configure);
    }
    
    public static void AddNuagesIdentity(this IdentityBuilder builder, IConfiguration? configuration = null, Action<NuagesIdentityOptions>? configure = null)
    {
        if (configuration != null)
        {
            builder.Services.Configure<NuagesIdentityOptions>(configuration.GetSection("Nuages:Identity"));
        }
        
        if (configure != null)
            builder.Services.Configure(configure);
        
        builder.Services.AddScoped(typeof(NuagesUserManager));
        builder.Services.AddScoped(typeof(NuagesSignInManager));


        builder.Services.AddScoped<ILookupProtector, LookupProtector>();
        builder.Services.AddScoped<ILookupProtectorKeyRing, LookupProtectorKeyRing>();
    }
}