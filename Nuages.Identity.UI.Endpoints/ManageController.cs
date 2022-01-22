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
    private readonly IChangePhoneNumberService _phoneNumberService;
    private readonly ISendSMSVerificationCode _sendSmsVerificationCode;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<ManageController> _logger;
    private readonly IStringLocalizer _stringLocalizer;

    public ManageController(IChangePasswordService changePasswordService, NuagesUserManager userManager, NuagesSignInManager signInManager,
        ISendEmailChangedConfirmationService sendEmailChangedConfirmationService, IChangeUserNameService changeUserNameService,
        IMFAService mfaService, IChangePhoneNumberService phoneNumberService, ISendSMSVerificationCode sendSmsVerificationCode,
        IWebHostEnvironment webHostEnvironment, ILogger<ManageController> logger, IStringLocalizer stringLocalizer)
    {
        _changePasswordService = changePasswordService;
        _userManager = userManager;
        _signInManager = signInManager;
        _sendEmailChangedConfirmationService = sendEmailChangedConfirmationService;
        _changeUserNameService = changeUserNameService;
        _mfaService = mfaService;
        _phoneNumberService = phoneNumberService;
        _sendSmsVerificationCode = sendSmsVerificationCode;
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
                AWSXRayRecorder.Instance.BeginSubsegment("ManageController.ChangePasswordAsync");

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
            else
            {
                _logger.LogError(e, "");
            }
            
            return new ChangePasswordResultModel
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
    
    [HttpPost("setPassword")]
    public async Task<ChangePasswordResultModel> SetPasswordAsync(ChangePasswordModel model)
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ManageController.SetPasswordAsync");

            _logger.LogInformation($"Initiate ChangePassword : Name = {User.Identity!.Name}  NewPassword = {model.NewPassword} NewPasswordConfirm = {model.NewPasswordConfirm}");

            var res = await _changePasswordService.AddPasswordAsync(User.Sub()!, model.NewPassword, model.NewPasswordConfirm);
            
            _logger.LogInformation($"Login Result : Success = {res.Success} Error = {res.Errors.FirstOrDefault()}");

            return res;
        }
        catch (Exception e)
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, "");
            }
            
            return new ChangePasswordResultModel
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
    
    [HttpPost("sendEmailChange")]
    public async Task<SendEmailChangeResultModel> SendEmailChangeMessageAsync([FromBody] SendEmailChangeModel model)
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ManageController.SendEmailChangeMessageAsync");

            var res = await _sendEmailChangedConfirmationService.SendEmailChangeConfirmation(User.Sub()!, model.Email);

            return res;
        }
        catch (Exception e)
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, "");
            }
            
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
                AWSXRayRecorder.Instance.BeginSubsegment("ManageController.ChangeUsernameAsync");
            
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
            else
            {
                _logger.LogError(e, "");
            }
            
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
    
    [HttpDelete("disable2Fa")]
    [AllowAnonymous]
    public async Task<DisableMFAResultModel> Disable2FaAsync()
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ManageController.Disable2FaAsync");
            
            var res = await _mfaService.DisableMFAAsync(User.Sub()!);

            
            return res;
        }
        catch (Exception e)
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, "");
            }
            
            return new DisableMFAResultModel
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
    
     
    [HttpPost("enable2FA")]
    [AllowAnonymous]
    public async Task<MFAResultModel> Enable2FaAsync([FromBody] EnableMFAModel model)
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ManageController.Enable2FaAsync");
            
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
                _logger.LogError(e, "");
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
    
    [HttpDelete("removePhone")]
    [AllowAnonymous]
    public async Task<ChangePhoneNumberResultModel> RemovePhoneAsync()
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ManageController.RemovePhoneAsync");
            
            var res = await _phoneNumberService.ChangePhoneNumberAsync(User.Sub()!, "", null);

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
                _logger.LogError(e, "");
            }
            
            return new ChangePhoneNumberResultModel
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
    
    [HttpPost("sendPhoneChangeMessage")]
    [AllowAnonymous]
    public async Task<SendSMSVerificationCodeResultModel> SendPhoneChangeVerificationAsync([FromBody] SendSMSVerificationCodeModel model)
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ManageController.SendPhoneChangeVerificationAsync");
            
            var res = await _sendSmsVerificationCode.SendCode(User.Sub()!, model.PhoneNumber);

            return res;
        }
        catch (Exception e)
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, "");
            }
            
            return new SendSMSVerificationCodeResultModel
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
    
    [HttpPost("changePhoneNumber")]
    [AllowAnonymous]
    public async Task<ChangePhoneNumberResultModel> ChangePhoneNUmberAsync([FromBody] ChangePhoneNumberModel model)
    {
        try
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ManageController.SendPhoneChangeVerificationAsync");
            
            var res = await _phoneNumberService.ChangePhoneNumberAsync(User.Sub()!, model.PhoneNumber, model.Token);

            return res;
        }
        catch (Exception e)
        {
            if (!_webHostEnvironment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, "");
            }
            
            return new ChangePhoneNumberResultModel
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