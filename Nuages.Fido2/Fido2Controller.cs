using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Nuages.Fido2;
using Nuages.Fido2.Models;

namespace Nuages.Identity.UI.Controllers;

[ApiController]
[Route("api/fido2")]
public class Fido2Controller : Controller
{
    private readonly IFido2Service _fido2Service;
    private readonly ILogger<Fido2Controller> _logger;

    public Fido2Controller(IFido2Service fido2Service, ILogger<Fido2Controller> logger)
    {
        _fido2Service = fido2Service;
        _logger = logger;
    }
    
    [HttpPost("makeCredentialOptions")]
    public async Task<JsonResult> MakeCredentialOptionsAsync([FromBody] MakeCredentialOptionsRequest makeCredentialOptionsRequest)
    {
        try
        {
            return Json( await _fido2Service.MakeCredentialOptionsAsync(makeCredentialOptionsRequest), new JsonSerializerOptions());
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }
    
    [HttpPost("registerNewCredential")]
    public async Task RegisrerNewCredentiakl([FromBody] RegisterCredentialRequest registerCredentialRequest)
    {
        try
        {
            await _fido2Service.RegisterNewCredentialAsync(registerCredentialRequest);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }
}