using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace Nuages.Identity.UI.OpenIdDict;

public static class OpenIdDictConfigExtensions
{
    public static void AddNuagesOpenIdDict(this IServiceCollection services, IConfiguration configuration, Action<OpenIdDictOptions> configure)
    {
        services.Configure<OpenIdDictOptions>(configuration.GetSection("Nuages:OpenIdDict"));
        services.Configure(configure);
        
        services.AddOpenIddict()
            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                var storage = configuration.GetValue<string>("Nuages:OpenIdDict:Storage");
                switch (storage)
                {
                    case "Mongo":
                    {
                        options.UseMongoDb()
                            .UseDatabase(new MongoClient(configuration["Nuages:OpenIdDict:ConnectionString"])
                                .GetDatabase(configuration["Nuages:OpenIdDict:Database"]));

                        break;
                    }
                    default:
                    {
                        services.AddDbContext<OpenIdDictContext>(contextOptions =>
                        {
                            contextOptions.UseInMemoryDatabase("IdentityContext");
                            contextOptions.UseOpenIddict();
                        });
                        
                        options.UseEntityFrameworkCore()
                            .UseDbContext<OpenIdDictContext>();
                        
                        break;
                    }
                }
                
            })
            // Register the OpenIddict server components.
            .AddServer(options =>
            {
#if DEBUG
                options.DisableAccessTokenEncryption();
#endif
                options.SetDeviceEndpointUris("/connect/device")
                    .SetVerificationEndpointUris("/connect/verify")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserinfoEndpointUris("/connect/userinfo")
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetLogoutEndpointUris("/connect/logout");
                
                // Mark the "email", "profile" and "roles" scopes as supported scopes.
                options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles);

                options.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow()
                    .AllowPasswordFlow()
                    .AllowDeviceCodeFlow()
                    .AllowClientCredentialsFlow();

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableVerificationEndpointPassthrough()
                #if DEBUG
                    .DisableTransportSecurityRequirement()
                #endif
                    .EnableStatusCodePagesIntegration()
                    ;
            })
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });

#if DEBUG
        services.AddHostedService<OpenIdDictInitializeWorker>();
#endif

        services.AddSingleton<IConfigureOptions<OpenIddictServerOptions>, OpenIddictServerOptionsInitializer>();
    }

}