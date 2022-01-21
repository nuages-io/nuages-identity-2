using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Manage;
using Nuages.Web;

namespace Nuages.Identity.UI.Endpoints;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ManageController : Controller
{
    private readonly IChangePasswordService _changePasswordService;
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly ISendEmailChangedConfirmationService _sendEmailChangedConfirmationService;
    private readonly IChangeUserNameService _changeUserNameService;
    private readonly IMFAService _mfaService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<ManageController> _logger;
    private readonly IStringLocalizer _stringLocalizer;

    public ManageController(IChangePasswordService changePasswordService, NuagesUserManager userManager, NuagesSignInManager signInManager,
        ISendEmailChangedConfirmationService sendEmailChangedConfirmationService, IChangeUserNameService changeUserNameService,
        IMFAService mfaService,
        IWebHostEnvironment webHostEnvironment, ILogger<ManageController> logger, IStringLocalizer stringLocalizer)
    {
        _changePasswordService = changePasswordService;
        _userManager = userManager;
        _signInManager = signInManager;
        _sendEmailChangedConfirmationService = sendEmailChangedConfirmationService;
        _changeUserNameService = changeUserNameService;
        _mfaService = mfaService;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
        _stringLocalizer = stringLocalizer;
    }
    
    [HttpPost("changePassword")]
    public async Task<ChangePasswordResultModel> ChangePasswordAsync(ChangePasswordModel model)
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.ChangePasswordAsync");

            _logger.LogInformation($"Initiate ChangePassword : Name = {User.Identity!.Name} {model.CurrentPassword} NewPassword = {model.NewPassword} NewPasswordConfirm = {model.NewPasswordConfirm}");

            var res = await _changePasswordService.ChangePasswordAsync(User.Sub()!, model.CurrentPassword, model.NewPassword, model.NewPasswordConfirm);
            
            _logger.LogInformation($"Login Result : Success = {res.Success} Error = {res.Errors.FirstOrDefault()}");

            if (res.Success)
            {
                var user = await _userManager.FindByIdAsync(User.Sub()!);
                await _signInManager.RefreshSignInAsync(user);
            }
            
            return res;
        }
        catch (Exception e)
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);

            return new ChangePasswordResultModel
            {
                Success = false,
                Message = _stringLocalizer["errorMessage:exception"]
            };
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

            var res = await _changePasswordService.AddPasswordAsync(User.Sub()!, model.NewPassword, model.NewPasswordConfirm);
            
            _logger.LogInformation($"Login Result : Success = {res.Success} Error = {res.Errors.FirstOrDefault()}");

            return res;
        }
        catch (Exception e)
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);

            return new ChangePasswordResultModel
            {
                Success = false,
                Message = _stringLocalizer["errorMessage:exception"]
            };
        }
        finally
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
    
    [HttpPost("sendEmailChange")]
    public async Task<SendEmailChangeResultModel> SendEmailChangeMessageAsync([FromBody] SendEmailChangeModel model)
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.SendEmailChangeMessageAsync");

            var res = await _sendEmailChangedConfirmationService.SendEmailChangeConfirmation(User.Sub()!, model.Email);

            return res;
        }
        catch (Exception e)
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);

            return new SendEmailChangeResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"]}
            };
        }
        finally
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
    
    
    [HttpPost("changeUsername")]
    [AllowAnonymous]
    public async Task<ChangeUserNameResultModel> ChangeUsernameAsync([FromBody] ChangeUserNameModel model)
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.ChangeUsernameAsync");
            
            var res = await _changeUserNameService.ChangeUserNameAsync(User.Sub()!, model.NewUserName);

            if (res.Success)
            {
                var user = await _userManager.FindByIdAsync(User.Sub()!);
                await _signInManager.RefreshSignInAsync(user);
            }
            
            return res;
        }
        catch (Exception e)
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);

            return new ChangeUserNameResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"]}
            };
        }
        finally
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
     
      
    }
    
    [HttpPost("verify2FACode")]
    [AllowAnonymous]
    public async Task<MFAResultModel> Verify2FaCodeAsync([FromBody] EnableMFAModel model)
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.Verify2FaCodeAsync");
            
            var res = await _mfaService.EnableMFAAsync(User.Sub()!, model.Code);

            if (res.Success)
            {
                var user = await _userManager.FindByIdAsync(User.Sub()!);
                await _signInManager.RefreshSignInAsync(user);
            }
            
            return res;
        }
        catch (Exception e)
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                throw;
            }
            
            return new MFAResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"]}
            };
        }
        finally
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
     
      
    }
}