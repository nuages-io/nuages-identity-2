using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

using Nuages.AspNetIdentity.Core;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Nuages.Identity.Services.OpenIdDict;

public class AuthorizationCodeFlowHandler : IAuthorizationCodeFlowHandler
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly IAudienceValidator _audienceValidator;
    private readonly IHttpContextAccessor _contextAccessor;

    public AuthorizationCodeFlowHandler(NuagesUserManager userManager, NuagesSignInManager signInManager, 
        IAudienceValidator audienceValidator,
        IHttpContextAccessor contextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _audienceValidator = audienceValidator;
        _contextAccessor = contextAccessor;
    }
    
    public async Task<IActionResult> ProcessAuthorizationCodeFlow(OpenIddictRequest openIdDictRequest)
    {
         // Retrieve the claims principal stored in the authorization code/device code/refresh token.
            var principal =
                (await _contextAccessor.HttpContext!.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))
                .Principal;

            // Retrieve the user profile corresponding to the authorization code/refresh token.
            // Note: if you want to automatically invalidate the authorization code/refresh token
            // when the user password/roles change, use the following line instead:
            // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
            {
                return new ForbidResult(
                     new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                     new AuthenticationProperties(new Dictionary<string, string?>
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
                return new ForbidResult(
                     new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                     new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The user is no longer allowed to sign in."
                    }));
            }

            var error = _audienceValidator.CheckAudience(openIdDictRequest, principal);
            if (!string.IsNullOrEmpty(error))
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = "invalid_audience",
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = error
                });

                return new ForbidResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties );
            }

            foreach (var claim in principal!.Claims)
            {
                claim.SetDestinations(ClaimsDestinations.GetDestinations(claim, principal));
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);
    }
    
   
}

public interface IAuthorizationCodeFlowHandler
{
    Task<IActionResult> ProcessAuthorizationCodeFlow(OpenIddictRequest openIdDictRequest);
}