using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.Services.AspNetIdentity;
using OpenIddict.Server.AspNetCore;

namespace Nuages.Identity.Services.OpenIdDict;

public class LogoutEndpoint  : ILogoutEndpoint
{
    private readonly NuagesSignInManager _signInManager;

    public LogoutEndpoint(NuagesSignInManager signInManager)
    {
        _signInManager = signInManager;
    }
    public async Task<IActionResult> Logout() 
    {
        // Ask ASP.NET Core Identity to delete the local and external cookies created
        // when the user agent is redirected from the external identity provider
        // after a successful authentication flow (e.g Google or Facebook).
        await _signInManager.SignOutAsync();

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent
        // to the post_logout_redirect_uri specified by the client application or to
        // the RedirectUri specified in the authentication properties if none was set.
        return new SignOutResult (
             new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
             new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }
}

public interface ILogoutEndpoint
{
    Task<IActionResult> Logout();
}