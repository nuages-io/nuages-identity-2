using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nuages.Identity.Services.AspNetIdentity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace Nuages.Identity.UI.OpenIdDict.Endpoints;

public class AuthorizeEndpoint : IAuthorizeEndpoint
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IOpenIddictServerRequestProvider _openIddictServerRequestProvider;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly NuagesUserManager _userManager;

    public AuthorizeEndpoint(NuagesUserManager userManager, NuagesSignInManager signInManager,
        IHttpContextAccessor contextAccessor,
        IOpenIddictApplicationManager applicationManager, IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager, IOpenIddictServerRequestProvider openIddictServerRequestProvider)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _contextAccessor = contextAccessor;
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _openIddictServerRequestProvider = openIddictServerRequestProvider;
    }

    public async Task<IActionResult> Authorize()
    {
        var openIdRequest = _openIddictServerRequestProvider.GetOpenIddictServerRequest() ??
                            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var httpRequest = _contextAccessor.HttpContext!.Request;

        // If prompt=login was specified by the client application,
        // immediately return the user agent to the login page.
        if (openIdRequest.HasPrompt(OpenIddictConstants.Prompts.Login))
        {
            // To avoid endless login -> authorization redirects, the prompt=login flag
            // is removed from the authorization request payload before redirecting the user.
            var prompt = string.Join(" ", openIdRequest.GetPrompts().Remove(OpenIddictConstants.Prompts.Login));

            var parameters = httpRequest.HasFormContentType
                ? httpRequest.Form.Where(parameter => parameter.Key != OpenIddictConstants.Parameters.Prompt).ToList()
                : httpRequest.Query.Where(parameter => parameter.Key != OpenIddictConstants.Parameters.Prompt).ToList();

            parameters.Add(KeyValuePair.Create(OpenIddictConstants.Parameters.Prompt, new StringValues(prompt)));

            return new ChallengeResult(
                new[] { IdentityConstants.ApplicationScheme },
                new AuthenticationProperties
                {
                    RedirectUri = httpRequest.PathBase + httpRequest.Path + QueryString.Create(parameters)
                });
        }

        // Retrieve the user principal stored in the authentication cookie.
        // If a max_age parameter was provided, ensure that the cookie is not too old.
        // If the user principal can't be extracted or the cookie is too old, redirect the user to the login page.
        var result = await _contextAccessor.HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if ( /*result == null ||*/ !result.Succeeded || openIdRequest.MaxAge != null &&
            result.Properties?.IssuedUtc != null &&
            DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(openIdRequest.MaxAge.Value))
        {
            // If the client application requested promptless authentication,
            // return an error indicating that the user is not logged in.
            if (openIdRequest.HasPrompt(OpenIddictConstants.Prompts.None))
                return new ForbidResult(
                    new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                    new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                    }));

            return new ChallengeResult(
                new[] { IdentityConstants.ApplicationScheme },
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
        var application = await _applicationManager.FindByClientIdAsync(openIdRequest.ClientId!) ??
                          throw new InvalidOperationException(
                              "Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        var authorizations = await _authorizationManager.FindAsync(
            user.Id,
            (await _applicationManager.GetIdAsync(application))!,
            OpenIddictConstants.Statuses.Valid,
            OpenIddictConstants.AuthorizationTypes.Permanent,
            openIdRequest.GetScopes()).ToListAsync();

        var principal =
            await _signInManager.CreateUserPrincipalAsync(user);

        // Note: in this sample, the granted scopes match the requested scope
        // but you may want to allow the user to uncheck specific scopes.
        // For that, simply restrict the list of scopes before calling SetScopes.
        principal.SetScopes(openIdRequest.GetScopes());
        principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());
        principal.SetAudiences(openIdRequest.Audiences);
        
        // Automatically create a permanent authorization 
        var authorization = authorizations.LastOrDefault() ?? await _authorizationManager.CreateAsync(
            principal,
            user.Id,
            (await _applicationManager.GetIdAsync(application))!,
            OpenIddictConstants.AuthorizationTypes.Permanent,
            principal.GetScopes());

        principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

        foreach (var claim in principal.Claims)
            claim.SetDestinations(ClaimsDestinations.GetDestinations(claim, principal));

        return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);
    }
}

public interface IAuthorizeEndpoint
{
    Task<IActionResult> Authorize();
}