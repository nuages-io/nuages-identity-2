using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Nuages.Identity.UI.OpenIdDict;

public class InitializeWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public InitializeWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("postman-ui", cancellationToken) is null)
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "postman-ui",
                ConsentType = ConsentTypes.Implicit,
                DisplayName = "Postman client application",
                Type = ClientTypes.Confidential,
                ClientSecret = "00000000-0000-0000-0000-000000000000",
                PostLogoutRedirectUris =
                {
                    new Uri("https://oauth.pstmn.io")
                },
                RedirectUris =
                {
                    new Uri("https://oauth.pstmn.io/v1/callback")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Device,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.GrantTypes.Password,
                    Permissions.GrantTypes.ClientCredentials,
                    // Permissions.GrantTypes.Implicit,
                    Permissions.GrantTypes.DeviceCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            }, cancellationToken);

        if (await manager.FindByClientIdAsync("device", cancellationToken) == null)
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "device",
                Type = ClientTypes.Public,
                ConsentType = ConsentTypes.Explicit,
                DisplayName = "Device client",
                Permissions =
                {
                    Permissions.GrantTypes.DeviceCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.Endpoints.Device,
                    Permissions.Endpoints.Token,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles
                }
            }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}