using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Nuages.Identity.UI.OpenIdDict.Endpoints.Handlers;

public class DeviceFlowHandler : IDeviceFlowHandler
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly NuagesIdentityOptions _options;
    private readonly NuagesSignInManager _signInManager;
    private readonly NuagesUserManager _userManager;

    public DeviceFlowHandler(NuagesUserManager userManager, NuagesSignInManager signInManager,
        IHttpContextAccessor contextAccessor, IOptions<NuagesIdentityOptions> options)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _contextAccessor = contextAccessor;
        _options = options.Value;
    }

    public async Task<IActionResult> ProcessDeviceFlow(OpenIddictRequest openIdDictRequest)
    {
        if (openIdDictRequest.IsDeviceCodeGrantType() || openIdDictRequest.IsRefreshTokenGrantType())
        {
            // Retrieve the claims principal stored in the authorization code/device code/refresh token.
            var principal =
                (await _contextAccessor.HttpContext!.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults
                    .AuthenticationScheme)).Principal;

            // Retrieve the user profile corresponding to the authorization code/refresh token.
            // Note: if you want to automatically invalidate the authorization code/refresh token
            // when the user password/roles change, use the following line instead:
            // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
            var user = await _userManager.GetUserAsync(principal!);
            if (user == null)
                return new ForbidResult(new List<string>
                    {
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                    },
                    new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The token is no longer valid."
                    }!));

            // Ensure the user is still allowed to sign in.
            if (!await _signInManager.CanSignInAsync(user))
                return new ForbidResult(
                    new List<string> { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                    new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The user is no longer allowed to sign in."
                    }!));

            foreach (var claim in principal?.Claims!)
                claim.SetDestinations(ClaimsDestinations.GetDestinations(claim, principal));

            if (principal != null && _options.Audiences != null)
            {
                if (openIdDictRequest.Audiences != null && openIdDictRequest.Audiences.Any())
                {
                    principal.SetAudiences(_options.Audiences.Intersect(openIdDictRequest.Audiences).Select(v => v!));
                }
                else
                {
                    principal.SetAudiences(_options.Audiences);
                }
            }
            
            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal!);
        }

        throw new Exception("Wrong grantType");
    }
}

public interface IDeviceFlowHandler
{
    Task<IActionResult> ProcessDeviceFlow(OpenIddictRequest openIdDictRequest);
}