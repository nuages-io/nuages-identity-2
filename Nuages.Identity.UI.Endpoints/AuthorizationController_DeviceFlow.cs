using OpenIddict.Abstractions;

namespace Nuages.Identity.UI.Endpoints;

public partial class AuthorizationController
{
    public  Task<Microsoft.AspNetCore.Mvc.SignInResult> ProcessDeviceFlow(OpenIddictRequest openIdDictRequest)
    {
        if (openIdDictRequest.IsDeviceCodeGrantType() || openIdDictRequest.IsRefreshTokenGrantType())
        {
            //     // Retrieve the claims principal stored in the authorization code/device code/refresh token.
            //     var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            //
            //     // Retrieve the user profile corresponding to the authorization code/refresh token.
            //     // Note: if you want to automatically invalidate the authorization code/refresh token
            //     // when the user password/roles change, use the following line instead:
            //     // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
            //     var user = await _userManager.GetUserAsync(principal);
            //     if (user == null)
            //     {
            //         return Forbid(
            //             authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            //             properties: new AuthenticationProperties(new Dictionary<string, string>
            //             {
            //                 [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
            //                 [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
            //             }));
            //     }
            //
            //     // Ensure the user is still allowed to sign in.
            //     if (!await _signInManager.CanSignInAsync(user))
            //     {
            //         return Forbid(
            //             authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            //             properties: new AuthenticationProperties(new Dictionary<string, string>
            //             {
            //                 [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
            //                 [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
            //             }));
            //     }
            //
            //     foreach (var claim in principal.Claims)
            //     {
            //         claim.SetDestinations(GetDestinations(claim, principal));
            //     }
            //
            //     // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            //     return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException();
    }
}