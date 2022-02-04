using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

using Nuages.AspNetIdentity.Core;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Nuages.Identity.Services.OpenIdDict;

public class AuthorizeEndpoint : IAuthorizeEndpoint
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;

    public AuthorizeEndpoint(NuagesUserManager userManager, NuagesSignInManager signInManager, IHttpContextAccessor contextAccessor,
        IOpenIddictApplicationManager applicationManager, IOpenIddictAuthorizationManager authorizationManager, IOpenIddictScopeManager scopeManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _contextAccessor = contextAccessor;
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
    }
    
    public async Task<IActionResult> Authorize()
    {
        var request = _contextAccessor.HttpContext!.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var httpRequest = _contextAccessor.HttpContext!.Request;
        
        // If prompt=login was specified by the client application,
        // immediately return the user agent to the login page.
        if (request.HasPrompt(OpenIddictConstants.Prompts.Login))
        {
            // To avoid endless login -> authorization redirects, the prompt=login flag
            // is removed from the authorization request payload before redirecting the user.
            var prompt = string.Join(" ", request.GetPrompts().Remove(OpenIddictConstants.Prompts.Login));

            var parameters = httpRequest.HasFormContentType
                ? httpRequest.Form.Where(parameter => parameter.Key != OpenIddictConstants.Parameters.Prompt).ToList()
                : httpRequest.Query.Where(parameter => parameter.Key != OpenIddictConstants.Parameters.Prompt).ToList();

            parameters.Add(KeyValuePair.Create(OpenIddictConstants.Parameters.Prompt, new StringValues(prompt)));

            return new ChallengeResult (
                 new[] { Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme },
                 new AuthenticationProperties
                {
                    RedirectUri = httpRequest.PathBase + httpRequest.Path + QueryString.Create(parameters)
                });
        }

        // Retrieve the user principal stored in the authentication cookie.
        // If a max_age parameter was provided, ensure that the cookie is not too old.
        // If the user principal can't be extracted or the cookie is too old, redirect the user to the login page.
        var result = await _contextAccessor.HttpContext.AuthenticateAsync(Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme);
        if ( /*result == null ||*/ !result.Succeeded || request.MaxAge != null &&
            result.Properties?.IssuedUtc != null &&
            DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value))
        {
            // If the client application requested promptless authentication,
            // return an error indicating that the user is not logged in.
            if (request.HasPrompt(OpenIddictConstants.Prompts.None))
            {
                return new ForbidResult (
                     new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                     new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                    }));
            }

            return new ChallengeResult (
                 new[] { Microsoft.AspNetCore.Identity.IdentityConstants.ApplicationScheme },
                 new AuthenticationProperties
                {
                    RedirectUri = httpRequest.PathBase + httpRequest.Path + QueryString.Create(
                        httpRequest.HasFormContentType ? httpRequest.Form.ToList() : httpRequest.Query.ToList())
                });
        }

        // Retrieve the profile of the logged in user.
        var user = await _userManager.GetUserAsync(result.Principal) ??
                   throw new InvalidOperationException("The user details cannot be retrieved.");

        // Retrieve the application details from the database.
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!) ??
                          throw new InvalidOperationException(
                              "Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        var authorizations = await _authorizationManager.FindAsync(
            user.Id,
            (await _applicationManager.GetIdAsync(application))!,
            OpenIddictConstants.Statuses.Valid,
            OpenIddictConstants.AuthorizationTypes.Permanent,
            request.GetScopes()).ToListAsync();
        
        var principal =
            await _signInManager.CreateUserPrincipalAsync(user);

        // Note: in this sample, the granted scopes match the requested scope
        // but you may want to allow the user to uncheck specific scopes.
        // For that, simply restrict the list of scopes before calling SetScopes.
        principal.SetScopes(request.GetScopes());
        principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

        // Automatically create a permanent authorization 
        var authorization = authorizations.LastOrDefault() ?? await _authorizationManager.CreateAsync(
            principal,
            user.Id,
            (await _applicationManager.GetIdAsync(application))!,
            OpenIddictConstants.AuthorizationTypes.Permanent,
            principal.GetScopes());

        principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(ClaimsDestinations.GetDestinations(claim, principal));
        }

        return new SignInResult (OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);
    }
}

public interface IAuthorizeEndpoint
{
    Task<IActionResult> Authorize();
}