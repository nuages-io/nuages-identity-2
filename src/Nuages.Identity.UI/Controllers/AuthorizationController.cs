using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.UI.OpenIdDict.Endpoints;
using OpenIddict.Server.AspNetCore;

namespace Nuages.Identity.UI.Controllers;

[ApiController]
[Route("connect")]
[IgnoreAntiforgeryToken]
[ApiExplorerSettings(IgnoreApi = true)]
public class AuthorizationController : Controller
{
    private readonly IAuthorizeEndpoint _authorizeEndpoint;
    private readonly ILogger<AuthorizationController> _logger;
    private readonly ILogoutEndpoint _logoutEndpoint;
    private readonly ITokenEndpoint _tokenEndpoint;
    private readonly IUserInfoEndpoint _userInfoEndpoint;

    public AuthorizationController(
        IAuthorizeEndpoint authorizeEndpoint,
        ILogoutEndpoint logoutEndpoint,
        ITokenEndpoint tokenEndpoint,
        IUserInfoEndpoint userInfoEndpoint,
        ILogger<AuthorizationController> logger)
    {
        _authorizeEndpoint = authorizeEndpoint;
        _logoutEndpoint = logoutEndpoint;
        _tokenEndpoint = tokenEndpoint;
        _userInfoEndpoint = userInfoEndpoint;
        _logger = logger;
    }

    [HttpPost("token")]
    [Produces("application/json")]
    public async Task<IActionResult> Token()
    {
        try
        {
            return await _tokenEndpoint.Exchange();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }

    [HttpGet("authorize")]
    [HttpPost("authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        try
        {
            return await _authorizeEndpoint.Authorize();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            return await _logoutEndpoint.Logout();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }
    
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("userinfo"), HttpPost("userinfo"), Produces("application/json")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UserIhfo()
    {
        try
        {
            return await _userInfoEndpoint.GetUserinfo();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }
}