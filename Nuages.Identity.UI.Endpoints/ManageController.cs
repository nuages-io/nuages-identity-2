using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.Manage;
using Nuages.Web;
using Nuages.Web.Recaptcha;

namespace Nuages.Identity.UI.Endpoints;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ManageController : Controller
{
    private readonly IChangePasswordService _changePasswordService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<ManageController> _logger;

    public ManageController(IChangePasswordService changePasswordService,
        IWebHostEnvironment webHostEnvironment, ILogger<ManageController> logger)
    {
        _changePasswordService = changePasswordService;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }
    
    [HttpPost("changePassword")]
    public async Task<ChangePasswordResultModel> ChangePasswordAsync(ChangePasswordModel model)
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.ChangePasswordAsync");

            _logger.LogInformation($"Initiate ChangePassword : Name = {User.Identity!.Name} {model.CurrentPassword} NewPassword = {model.NewPassword} NewPasswordConfirm = {model.NewPasswordConfirm}");

            var res = await _changePasswordService.ChangePasswordAsync(User.Sub()!, model);
            
            _logger.LogInformation($"Login Result : Success = {res.Success} Error = {res.Errors.FirstOrDefault()}");

            return res;
        }
        catch (Exception e)
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);

            throw;
        }
        finally
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
    
    [HttpPost("setPassword")]
    public async Task<ChangePasswordResultModel> SetPasswordAsync(ChangePasswordModel model)
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.SetPasswordAsync");

            _logger.LogInformation($"Initiate ChangePassword : Name = {User.Identity!.Name}  NewPassword = {model.NewPassword} NewPasswordConfirm = {model.NewPasswordConfirm}");

            var res = await _changePasswordService.ChangePasswordAsync(User.Sub()!, model);
            
            _logger.LogInformation($"Login Result : Success = {res.Success} Error = {res.Errors.FirstOrDefault()}");

            return res;
        }
        catch (Exception e)
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);

            throw;
        }
        finally
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
}