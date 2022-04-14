using Fido2NetLib;

namespace Nuages.Identity.Services.Fido2;

public static class Fido2Extension
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IFido2Builder AddNuagesFido2(this IServiceCollection services,  Action<Fido2Configuration> setupAction)
    {
        services.AddScoped<IFido2Service, Fido2Service>();
        
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

        var fido2 = services.AddFido2(setupAction);

        var options = new Fido2Configuration();
        setupAction.Invoke(options);

        if (!string.IsNullOrEmpty(options.MDSCacheDirPath))
        {
            fido2.AddCachedMetadataService(config =>
            {
                config.AddFidoMetadataRepository();
            });
        }

        return new Fido2Builder(services);
    }
}