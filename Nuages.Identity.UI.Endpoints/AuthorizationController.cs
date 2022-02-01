using System.Diagnostics.CodeAnalysis;
using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.Services.OpenIdDict;

namespace Nuages.Identity.UI.Endpoints;

[ExcludeFromCodeCoverage]
public class AuthorizationController : Controller
{
    private readonly IAuthorizeEndpoint _authorizeEndpoint;
    private readonly ILogoutEndpoint _logoutEndpoint;
    private readonly ITokenEndpoint _tokenEndpoint;
    private readonly ILogger<AuthorizationController> _logger;

    public AuthorizationController(
        IAuthorizeEndpoint authorizeEndpoint,
        ILogoutEndpoint logoutEndpoint,
        ITokenEndpoint tokenEndpoint, ILogger<AuthorizationController> logger)
    {
        _authorizeEndpoint = authorizeEndpoint;
        _logoutEndpoint = logoutEndpoint;
        _tokenEndpoint = tokenEndpoint;
        _logger = logger;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment("AuthorizationController.Authorize");
            
            return await _authorizeEndpoint.Authorize();
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            throw;
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpGet("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment("AuthorizationController.Logout");
            
            return await _logoutEndpoint.Logout();
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            throw;
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("~/connect/token"), Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment("AuthorizationController.Exchange");
            
            return await _tokenEndpoint.Exchange();
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            throw;
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
}