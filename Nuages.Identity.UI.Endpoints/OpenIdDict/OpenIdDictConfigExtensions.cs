using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace Nuages.Identity.UI.Endpoints.OpenIdDict;

public static class OpenIdDictConfigExtensions
{
    public static void AddNuagesOpenIdDict(this IServiceCollection services, string connectionString, string databaseName = "openiddict")
    {
        // services.Configure<IdentityOptions>(options =>
        // {
        //     options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
        //     options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
        //     options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
        // });
        
        services.AddOpenIddict()
            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                options.UseMongoDb()
                    .UseDatabase(new MongoClient(connectionString)
                    .GetDatabase("openiddict"));
            })

            // Register the OpenIddict server components.
            .AddServer(options =>
            {
#if DEBUG
                options.DisableAccessTokenEncryption();
#endif
                // Enable the authorization, logout, token and userinfo endpoints.
                options.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetLogoutEndpointUris("/connect/logout")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserinfoEndpointUris("/connect/userinfo");

                // Mark the "email", "profile" and "roles" scopes as supported scopes.
                options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles);

                // Note: the sample uses the code and refresh token flows but you can enable
                // the other flows if you need to support implicit, password or client credentials.
                options.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow()
                    .AllowPasswordFlow()
                    .AllowClientCredentialsFlow();

                // Register the signing and encryption credentials.
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                // options.AddEphemeralSigningKey();
                // options.AddEphemeralEncryptionKey();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableStatusCodePagesIntegration()
                    .EnableTokenEndpointPassthrough();
            })

            // Register the OpenIddict validation components.
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