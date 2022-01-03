using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Nuages.Identity.UI.Endpoints;

public partial class AuthorizationController
{
    private async Task<IActionResult> ProcessAuthorizationCodeFlow(OpenIddictRequest openIdDictRequest)
    {
        if (openIdDictRequest.IsAuthorizationCodeGrantType() || openIdDictRequest.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the authorization code/device code/refresh token.
            var principal =
                (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))
                .Principal;

            // Retrieve the user profile corresponding to the authorization code/refresh token.
            // Note: if you want to automatically invalidate the authorization code/refresh token
            // when the user password/roles change, use the following line instead:
            // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The token is no longer valid."
                    }));
            }

            // Ensure the user is still allowed to sign in.
            if (!await _signInManager.CanSignInAsync(user))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The user is no longer allowed to sign in."
                    }));
            }

            if (openIdDictRequest.Audiences != null)
            {
                foreach (var audience in openIdDictRequest.Audiences)
                {
                    if (IsValidAudience(audience))
                    {
                        (principal!.Identity as ClaimsIdentity ?? throw new InvalidOperationException())
                            .AddClaim("aud", audience);
                    }
                    else
                    {
                        throw new Exception("Invalid Audience provided");
                    }
                }
            }
            else
            {
                if (HasAudiences)
                    throw new Exception("Audience must be provided");
            }

            foreach (var claim in principal!.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new Exception("Wrong grant type");
    }

    private bool IsValidAudience(string audience)
    {
        return true;
    }

    private bool HasAudiences
    {
        get
        {
            return true;
        }
    }
}