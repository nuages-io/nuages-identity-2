using Amazon.SimpleEmailV2.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Nuages.Identity.UI.OpenIdDict.Endpoints.Handlers;

public class PasswordFlowHandler : IPasswordFlowHandler
{
    private readonly IAudienceValidator _audienceValidator;
    private readonly NuagesSignInManager _signInManager;
    private readonly NuagesUserManager _userManager;
    private readonly NuagesIdentityOptions _options;

    public PasswordFlowHandler(NuagesSignInManager signInManager, NuagesUserManager userManager,
        IAudienceValidator audienceValidator, IOptions<NuagesIdentityOptions> options)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _audienceValidator = audienceValidator;
        _options = options.Value;
    }

    public async Task<IActionResult> ProcessPasswordFlow(OpenIddictRequest request)
    {
        if (request.IsPasswordGrantType())
        {
            // Validate the username/password parameters and ensure the account is not locked out.
            var result =
                await _signInManager.PasswordSignInAsync(request.Username!, request.Password!,
                    true, true);
            if (!result.Succeeded)
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The username/password couple is invalid."
                });

                return new ForbidResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties);
            }

            var user = await _userManager.FindByNameAsync(request.Username!);
            if (user == null)
                throw new NotFoundException("User not found");
            
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Set the list of scopes granted to the client application.
            principal.SetScopes(new[]
            {
                OpenIddictConstants.Scopes.OpenId,
                OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Scopes.Profile,
                OpenIddictConstants.Scopes.Roles
            }.Intersect(request.GetScopes()));

            
            var error = _audienceValidator.CheckAudience(request, principal);
            if (!string.IsNullOrEmpty(error))
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = "invalid_audience",
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = error
                });

                return new ForbidResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties);
            }

            if (_options.Audiences != null)
            {
                if (request.Audiences != null && request.Audiences.Any())
                {
                    principal.SetAudiences(_options.Audiences.Intersect(request.Audiences).Select(v =>v!));
                }
                else
                {
                    principal.SetAudiences(_options.Audiences);
                }
            }
            
            
            
;            foreach (var claim in principal.Claims)
                claim.SetDestinations(ClaimsDestinations.GetDestinations(claim, principal));

            return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);
        }

        throw new Exception("Wrong grant type");
    }
}

public interface IPasswordFlowHandler
{
    Task<IActionResult> ProcessPasswordFlow(OpenIddictRequest request);
}