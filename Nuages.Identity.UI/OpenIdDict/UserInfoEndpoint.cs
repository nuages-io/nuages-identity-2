using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.Services.AspNetIdentity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Nuages.Identity.UI.OpenIdDict;

public class UserInfoEndpoint : IUserInfoEndpoint
{
    private readonly NuagesUserManager _userManager;
    private readonly IHttpContextAccessor _contextAccessor;

    public UserInfoEndpoint(NuagesUserManager userManager, IHttpContextAccessor contextAccessor)
    {
        _userManager = userManager;
        _contextAccessor = contextAccessor;
    }
    
    public async Task<IActionResult> GetUserinfo()
    {
        var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext!.User);
        if (user == null)
        {
            return new ChallengeResult(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is bound to an account that no longer exists."
                }!));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            [OpenIddictConstants.Claims.Subject] = await _userManager.GetUserIdAsync(user)
        };

        if (_contextAccessor.HttpContext!.User.HasScope(OpenIddictConstants.Scopes.Email))
        {
            claims[OpenIddictConstants.Claims.Email] = await _userManager.GetEmailAsync(user);
        }

        if (_contextAccessor.HttpContext!.User.HasScope(OpenIddictConstants.Scopes.Roles))
        {
            claims[OpenIddictConstants.Claims.Role] = await _userManager.GetRolesAsync(user);
        }

        // Note: the complete list of standard claims supported by the OpenID Connect specification
        // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

        return new OkObjectResult(claims);
    }
}

public interface IUserInfoEndpoint
{
    Task<IActionResult> GetUserinfo();
}
 