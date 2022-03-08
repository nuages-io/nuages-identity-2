using Fido2NetLib;

namespace Nuages.Fido2;

public static class Fido2Extension
{
    public static void AddNuagesFido2(this IServiceCollection services,  Action<Fido2Configuration> setupAction)
    {
        services.AddScoped<IFido2Service, Fido2Service>();
        services.AddScoped<IFido2Storage, Fido2InMemoryStorage>();
        
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            // Set a short timeout for easy testing.
            options.IdleTimeout = TimeSpan.FromMinutes(2);
            options.Cookie.HttpOnly = true;
            // Strict SameSite mode is required because the default mode used
            // by ASP.NET Core 3 isn't understood by the Conformance Tool
            // and breaks conformance testing
            options.Cookie.SameSite = SameSiteMode.Unspecified;
        });

        services.AddFido2(setupAction )
        .AddCachedMetadataService(config =>
        {
            config.AddFidoMetadataRepository();
        });
    }
}