using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.Services;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Nuages.Identity.UI.Controllers;

public partial class AuthorizationController
{
    private async Task<IActionResult> ProcessPasswordFlow(OpenIddictRequest request)
    {
        if (request.IsPasswordGrantType())
        {
            var user = await _userManager.FindAsync(request.Username!);
            if (user.FindMode == FindUserMode.NotFound)
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The username/password couple is invalid."
                });

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Validate the username/password parameters and ensure the account is not locked out.
            var result =
                await _signInManager.CheckPasswordSignInAsync(user.User!, request.Password!,
                    true);
            if (!result.Succeeded)
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The username/password couple is invalid."
                });

                return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await _signInManager.CreateUserPrincipalAsync(user.User!);

            // Set the list of scopes granted to the client application.
            principal.SetScopes(new[]
            {
                Scopes.OpenId,
                Scopes.Email,
                Scopes.Profile,
                Scopes.Roles
            }.Intersect(request.GetScopes()));

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new Exception("Wrong grant type");
    }

}