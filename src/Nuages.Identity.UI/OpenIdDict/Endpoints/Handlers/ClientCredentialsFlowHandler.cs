using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nuages.Identity.Services.AspNetIdentity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Nuages.Identity.UI.OpenIdDict.Endpoints.Handlers;

public class ClientCredentialsFlowHandler : IClientCredentialsFlowHandler
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IAudienceValidator _audienceValidator;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly NuagesIdentityOptions _options;

    public ClientCredentialsFlowHandler(IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        IAudienceValidator audienceValidator, IOptions<NuagesIdentityOptions> options)
    {
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
        _audienceValidator = audienceValidator;
        _options = options.Value;
    }

    public async Task<IActionResult> ProcessClientCredentialsFlow(
        OpenIddictRequest openIdDictRequest)
    {
        if (openIdDictRequest.IsClientCredentialsGrantType())
        {
            // Note: the client credentials are automatically validated by OpenIddict:
            // if client_id or client_secret are invalid, this action won't be invoked.

            var application = await _applicationManager.FindByClientIdAsync(openIdDictRequest.ClientId!);
            if (application == null)
                throw new InvalidOperationException("The application details cannot be found in the database.");

            // Create a new ClaimsIdentity containing the claims that
            // will be used to create an id_token, a token or a code.
            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role);

            var clientId = await _applicationManager.GetClientIdAsync(application);

            if (string.IsNullOrEmpty(clientId))
                throw new Exception("Unabel to get clientId");

            var name = await _applicationManager.GetDisplayNameAsync(application);

            if (string.IsNullOrEmpty(name))
                throw new Exception("Unabel to get client name");

            // Use the client_id as the subject identifier.
            identity.AddClaim(OpenIddictConstants.Claims.Subject, clientId/*,
                OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken*/);

            identity.AddClaim(OpenIddictConstants.Claims.Name, name/*,
                OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken*/);

            // Note: In the original OAuth 2.0 specification, the client credentials grant
            // doesn't return an identity token, which is an OpenID Connect concept.
            //
            // As a non-standardized extension, OpenIddict allows returning an id_token
            // to convey information about the client application when the "openid" scope
            // is granted (i.e specified when calling principal.SetScopes()). When the "openid"
            // scope is not explicitly set, no identity token is returned to the client application.

            // Set the list of scopes granted to the client application in access_token.
            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(openIdDictRequest.GetScopes());
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

            var error = _audienceValidator.CheckAudience(openIdDictRequest, principal);
            if (!string.IsNullOrEmpty(error))
            {
                var properties = new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = "invalid_audience",
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = error
                });

                return new ForbidResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties);
            }

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
            
            foreach (var claim in principal!.Claims)
            {
                claim.SetDestinations(ClaimsDestinations.GetDestinations(claim));
            }

            return new SignInResult(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);
        }

        throw new Exception("Wrong grantType");
    }
}

public interface IClientCredentialsFlowHandler
{
    Task<IActionResult> ProcessClientCredentialsFlow(
        OpenIddictRequest openIdDictRequest);
}