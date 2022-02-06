using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace Nuages.Identity.UI.OpenIdDict;

public class TokenEndpoint  : ITokenEndpoint
{
    private readonly IAuthorizationCodeFlowHandler _codeFlowHandler;
    private readonly IClientCredentialsFlowHandler _credentialsFlowHandler;
    private readonly IDeviceFlowHandler _deviceFlowHandler;
    private readonly IPasswordFlowHandler _passwordFlowHandler;
    private readonly IHttpContextAccessor _contextAccessor;

    public TokenEndpoint( IAuthorizationCodeFlowHandler codeFlowHandler,
        IClientCredentialsFlowHandler credentialsFlowHandler,
        IDeviceFlowHandler deviceFlowHandler,
        IPasswordFlowHandler  passwordFlowHandler, IHttpContextAccessor contextAccessor)
    {
        _codeFlowHandler = codeFlowHandler;
        _credentialsFlowHandler = credentialsFlowHandler;
        _deviceFlowHandler = deviceFlowHandler;
        _passwordFlowHandler = passwordFlowHandler;
        _contextAccessor = contextAccessor;
    }
    public async Task<IActionResult> Exchange()
    {
        var request = _contextAccessor.HttpContext!.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        switch (request.GrantType)
        {
            case OpenIddictConstants.GrantTypes.ClientCredentials:
            {
                return await _credentialsFlowHandler.ProcessClientCredentialsFlow(request);
            }
            case OpenIddictConstants.GrantTypes.DeviceCode:
            {
                return await _deviceFlowHandler.ProcessDeviceFlow(request);
            }
            case OpenIddictConstants.GrantTypes.Password:
            {
                return await _passwordFlowHandler.ProcessPasswordFlow(request);
            }
            case OpenIddictConstants.GrantTypes.AuthorizationCode:
            case OpenIddictConstants.GrantTypes.RefreshToken:
            {
                return await _codeFlowHandler.ProcessAuthorizationCodeFlow(request);
            }
        }

        throw new InvalidOperationException("The specified grant type lol is not supported.");
    }
}

public interface ITokenEndpoint
{
    Task<IActionResult> Exchange();
}