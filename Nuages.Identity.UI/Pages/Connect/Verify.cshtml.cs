using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.UI.OpenIdDict;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Connect;

public class Verify : PageModel
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly NuagesUserManager _userManager;

    public Verify(NuagesUserManager userManager, NuagesSignInManager signInManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
    }

    public string UserCode { get; set; } = string.Empty;

    public string Error { get; set; } = string.Empty;
    public string ErrorDescription { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;

    public async Task<IActionResult> OnGet()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // If the user code was not specified in the query string (e.g as part of the verification_uri_complete),
        // render a form to ask the user to enter the user code manually (non-digit chars are automatically ignored).
        if (string.IsNullOrEmpty(request.UserCode)) return Page();

        // Retrieve the claims principal associated with the user code.
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (result.Succeeded)
        {
            // Retrieve the application details from the database using the client_id stored in the principal.
            var application =
                await _applicationManager.FindByClientIdAsync(
                    result.Principal.GetClaim(OpenIddictConstants.Claims.ClientId)!) ??
                throw new InvalidOperationException(
                    "Details concerning the calling client application cannot be found.");

            ApplicationName = await _applicationManager.GetLocalizedDisplayNameAsync(application) ?? string.Empty;
            Scope = string.Join(" ", result.Principal.GetScopes());
            UserCode = request.UserCode;

            // Render a form asking the user to confirm the authorization demand.
            return Page();
        }

        Error = OpenIddictConstants.Errors.InvalidToken;
        ErrorDescription = "The specified user code is not valid. Please make sure you typed it correctly.";
        // Redisplay the form when the user code is not valid.
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (HttpContext.Request.Form.ContainsKey("submit.Accept"))
        {
            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(User) ??
                       throw new InvalidOperationException("The user details cannot be retrieved.");

            // Retrieve the claims principal associated with the user code.
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (result.Succeeded)
            {
                var principal = await _signInManager.CreateUserPrincipalAsync(user);

                // Note: in this sample, the granted scopes match the requested scope
                // but you may want to allow the user to uncheck specific scopes.
                // For that, simply restrict the list of scopes before calling SetScopes.
                principal.SetScopes(result.Principal.GetScopes());
                principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

                foreach (var claim in principal.Claims)
                    claim.SetDestinations(ClaimsDestinations.GetDestinations(claim, principal));

                var properties = new AuthenticationProperties
                {
                    // This property points to the address OpenIddict will automatically
                    // redirect the user to after validating the authorization demand.
                    RedirectUri = "/Connect/VerifyDone"
                };

                return SignIn(principal, properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            Error = "InvalidToken";
            ErrorDescription = "The specified user code is not valid. Please make sure you typed it correctly.";

            // Redisplay the form when the user code is not valid.
            return Page();
        }

        return Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                // This property points to the address OpenIddict will automatically
                // redirect the user to after rejecting the authorization demand.
                RedirectUri = "/Connect/VerifyDone?success=false"
            });
    }
}