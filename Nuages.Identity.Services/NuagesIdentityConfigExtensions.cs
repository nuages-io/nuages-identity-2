
using Microsoft.AspNetCore.Identity;
using Nuages.Identity.Services.AspNetIdentity;

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
        var services = builder.Services;
        
        if (configuration != null)
        {
            services.Configure<NuagesIdentityOptions>(configuration.GetSection("Nuages:Identity"));
        }
        
        if (configure != null)
            services.Configure(configure);
        
        services.AddScoped(typeof(NuagesUserManager));
        services.AddScoped(typeof(NuagesSignInManager));

        services.AddScoped<ILoginService, LoginService>();
    
        services.AddScoped<ILookupProtector, LookupProtector>();
        services.AddScoped<ILookupProtectorKeyRing, LookupProtectorKeyRing>();
    }
}