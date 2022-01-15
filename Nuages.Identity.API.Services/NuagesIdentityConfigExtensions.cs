using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.Identity.Services;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Sender.API.Sdk;

namespace Nuages.Identity.API.Services;

public static class NuagesIdentityConfigExtensions
{
    public static void AddNuagesIdentityApi(this IdentityBuilder builder,
        Action<NuagesIdentityOptions> configure )
    {
        AddNuagesIdentityAPI(builder, null, configure);
    }
    
    public static void AddNuagesIdentityAPI(this IdentityBuilder builder, IConfiguration? configuration = null, Action<NuagesIdentityOptions>? configure = null)
    {
        var services = builder.Services;
        
        if (configuration != null)
        {
            services.Configure<NuagesIdentityOptions>(configuration.GetSection("Nuages:Identity"));
        }
        
        services.AddSenderClient(configuration);
        
        if (configure != null)
            services.Configure(configure);
        
        services.AddScoped(typeof(NuagesUserManager));
        services.AddScoped(typeof(NuagesSignInManager));

    }
}