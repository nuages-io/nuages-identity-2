using System.Text;
using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.Services.Fido2;
using Nuages.Identity.Services.Fido2.Models;
using Nuages.Web;

namespace Nuages.Identity.UI.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("app/fido2")]
public class Fido2Controller : Controller
{
    private readonly IFido2Service _fido2Service;
    private readonly IFido2SignInManager _signInManager;
    private readonly ILogger<Fido2Controller> _logger;

    public Fido2Controller(IFido2Service fido2Service, IFido2SignInManager signInManager, ILogger<Fido2Controller> logger)
    {
        _fido2Service = fido2Service;
        _signInManager = signInManager;
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
    public async Task<Fido2NetLib.Fido2.CredentialMakeResult> MakeCredential([FromBody] AuthenticatorAttestationRawResponse attestationResponse)
    {
        try
        {
            return await _fido2Service.MakeNewCredentialAsync(attestationResponse);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }
    
    [HttpPost]
    [Route("assertionOptions")]
    public async Task<JsonResult> AssertionOptionsPost([FromBody] AssertionOptionsRequest request)
    {
        try
        {
            try
            {
                var options = await _fido2Service.AssertionOptionAsync(request);

                return Json(options, new JsonSerializerOptions());
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);

                throw;
            }
        }

        catch (Exception e)
        {
            return Json(new AssertionOptions { Status = "error", ErrorMessage = e.Message });
        }
    }
    
    [HttpPost]
    [Route("makeAssertion")]
    [ValidateAntiForgeryToken]
    public async Task<JsonResult> MakeAssertion([FromBody] AuthenticatorAssertionRawResponse clientResponse)
    {
        try
        {
            var res = await _fido2Service.MakeAssertionAsync(clientResponse);

            if (res.Status == "ok")
            {
                await _signInManager.SignInAsync();
            }
                
            return Json(res, new JsonSerializerOptions());
        }
        catch (Exception e)
        {
            return Json(new AssertionVerificationResult { Status = "error", ErrorMessage = e.Message });
        }
    }

    [HttpDelete]
    [Route("removeKey")]
    public async Task<bool> RemoveKey([FromBody] RemoveCredentialRequest request)
    {
        try
        {
            await _fido2Service.RemoveKeyAsync(Encoding.UTF8.GetBytes(User.Sub()!), Convert.FromBase64String(request.Id));
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}