using System.Text.Json;
using Fido2NetLib;
using Microsoft.AspNetCore.Mvc;
using Nuages.Fido2.Models;

namespace Nuages.Fido2;

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
    
    [HttpPost("makeCredential")]
    public async Task<Fido2NetLib.Fido2.CredentialMakeResult> MakeCredential([FromBody] AuthenticatorAttestationRawResponse attestationResponse, CancellationToken cancellationToken)
    {
        try
        {
            return await _fido2Service.MakeNewCredentialAsync(attestationResponse, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }
}