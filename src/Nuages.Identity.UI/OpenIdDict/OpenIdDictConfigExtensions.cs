using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.UI.OpenIdDict.Endpoints;
using Nuages.Identity.UI.OpenIdDict.Endpoints.Handlers;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace Nuages.Identity.UI.OpenIdDict;

public static class OpenIdDictConfigExtensions
{
    public static void AddNuagesOpenIdDict(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAudienceValidator, AudienceValidator>();
        services.AddScoped<IAuthorizationCodeFlowHandler, AuthorizationCodeFlowHandler>();
        services.AddScoped<IAuthorizeEndpoint, AuthorizeEndpoint>();
        services.AddScoped<IClientCredentialsFlowHandler, ClientCredentialsFlowHandler>();
        services.AddScoped<IDeviceFlowHandler, DeviceFlowHandler>();
        services.AddScoped<ILogoutEndpoint, LogoutEndpoint>();
        services.AddScoped<IPasswordFlowHandler, PasswordFlowHandler>();
        services.AddScoped<ITokenEndpoint, TokenEndpoint>();
        services.AddScoped<IUserInfoEndpoint, UserInfoEndpoint>();

        services.AddScoped<IOpenIddictServerRequestProvider, OpenIdDictRequestProvider>();
        
        
        
        services.AddOpenIddict()
            // Register the OpenIddict core components.
            .AddCore(options =>
            {
                var storage = configuration.GetValue<string>("Nuages:Data:Storage");
                
                switch (storage)
                {
                    case "MongoDb":
                    {
                        var connectionString = configuration["Nuages:Data:ConnectionString"];
                        
                        var url = new MongoUrl(connectionString);
                        
                        options.UseMongoDb()
                            .UseDatabase(new MongoClient(connectionString)
                                .GetDatabase(url.DatabaseName));

                        break;
                    }
                    case "InMemory":
                    case "SqlServer":
                    case "MySql":
                    {
                        options.UseEntityFrameworkCore()
                            .UseDbContext<NuagesIdentityDbContext>();

                        break;
                    }
                    default:
                    {
                        throw new Exception("Storage not supported");
                    }
                }
            })
            // Register the OpenIddict server components.
            .AddServer(options =>
            {
                // var url = new Uri(configuration["Nuages:Identity:Authority"]);
                // options.SetIssuer(url);
                
                options.DisableAccessTokenEncryption();

                options.SetAccessTokenLifetime(TimeSpan.FromDays(1));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(1));
                
                options.SetDeviceEndpointUris("/connect/device")
                    .SetVerificationEndpointUris("/connect/verify")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserinfoEndpointUris("/connect/userinfo")
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetLogoutEndpointUris("/connect/logout");

                // Mark the "email", "profile" and "roles" scopes as supported scopes.
                options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,  OpenIddictConstants.Scopes.OpenId);

                options.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow()
                    .AllowPasswordFlow()
                    .AllowDeviceCodeFlow()
                    .AllowClientCredentialsFlow();

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough()
                    .EnableVerificationEndpointPassthrough() 
#if DEBUG
                    .DisableTransportSecurityRequirement()
#endif
                    .EnableStatusCodePagesIntegration();
            })
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });

        if (configuration.GetValue<bool>("Nuages:OpenIdDict:CreateDemoClients"))
        {
            services.AddHostedService<InitializeWorker>();
        }
        
        services.AddSingleton<IConfigureOptions<OpenIddictServerOptions>, ServerOptionsInitializer>();
    }
}