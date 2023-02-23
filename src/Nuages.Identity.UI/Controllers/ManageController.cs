using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Manage;
using Nuages.Web;

namespace Nuages.Identity.UI.Controllers;

[Authorize]
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("app/[controller]")]
public class ManageController : Controller
{
    private readonly IChangePasswordService _changePasswordService;
    private readonly IChangeUserNameService _changeUserNameService;
    private readonly ILogger<ManageController> _logger;
    private readonly IMFAService _mfaService;
    private readonly IChangePhoneNumberService _phoneNumberService;
    private readonly IProfileService _profileService;
    private readonly ISendEmailChangeConfirmationService _sendEmailChangedConfirmationService;
    private readonly ISendSMSVerificationCodeService _sendSmsVerificationCode;
    private readonly NuagesSignInManager _signInManager;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly NuagesUserManager _userManager;

    public ManageController(IChangePasswordService changePasswordService, NuagesUserManager userManager,
        NuagesSignInManager signInManager,
        ISendEmailChangeConfirmationService sendEmailChangedConfirmationService,
        IChangeUserNameService changeUserNameService,
        IMFAService mfaService, IChangePhoneNumberService phoneNumberService,
        ISendSMSVerificationCodeService sendSmsVerificationCode,
        IProfileService profileService,
        ILogger<ManageController> logger, IStringLocalizer stringLocalizer)
    {
        _changePasswordService = changePasswordService;
        _userManager = userManager;
        _signInManager = signInManager;
        _sendEmailChangedConfirmationService = sendEmailChangedConfirmationService;
        _changeUserNameService = changeUserNameService;
        _mfaService = mfaService;
        _phoneNumberService = phoneNumberService;
        _sendSmsVerificationCode = sendSmsVerificationCode;
        _profileService = profileService;
        _logger = logger;
        _stringLocalizer = stringLocalizer;
    }

    [HttpPost("changePassword")]
    public async Task<ChangePasswordResultModel> ChangePasswordAsync([FromBody] ChangePasswordModel model)
    {
        try
        {
            _logger.LogInformation(
                "Initiate ChangePassword : Name = {Name} {CurrentPassword} NewPassword = {NewPassword} NewPasswordConfirm = {NewPasswordConfirm}",User.Identity!.Name,model.CurrentPassword,model.NewPassword,model.NewPasswordConfirm);

            var res = await _changePasswordService.ChangePasswordAsync(User.Sub()!, model.CurrentPassword,
                model.NewPassword, model.NewPasswordConfirm);

            _logger.LogInformation("Login Result : Success = {Success} Error = {Error}",res.Success,res.Errors.FirstOrDefault());

            if (res.Success)
            {
                var user = await _userManager.FindByIdAsync(User.Sub()!);
                if (user != null)
                    await _signInManager.RefreshSignInAsync(user);
            }

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            return new ChangePasswordResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [HttpPost("setPassword")]
    public async Task<ChangePasswordResultModel> SetPasswordAsync([FromBody] ChangePasswordModel model)
    {
        try
        {
            _logger.LogInformation(
                "Initiate ChangePassword : Name = {Name}  NewPassword = {NewPassword} NewPasswordConfirm = {NewPasswordConfirm}",User.Identity!.Name,model.NewPassword,model.NewPasswordConfirm);

            var res = await _changePasswordService.AddPasswordAsync(User.Sub()!, model.NewPassword,
                model.NewPasswordConfirm);

            _logger.LogInformation("Login Result : Success = {Success} Error = {Error}",res.Success,res.Errors.FirstOrDefault());

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            return new ChangePasswordResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [HttpPost("sendEmailChange")]
    public async Task<SendEmailChangeResultModel> SendEmailChangeMessageAsync([FromBody] SendEmailChangeModel model)
    {
        try
        {
            var res = await _sendEmailChangedConfirmationService.SendEmailChangeConfirmation(User.Sub()!, model.Email);

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            return new SendEmailChangeResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [HttpPost("changeUsername")]
    public async Task<ChangeUserNameResultModel> ChangeUsernameAsync([FromBody] ChangeUserNameModel model)
    {
        try
        {
            var res = await _changeUserNameService.ChangeUserNameAsync(User.Sub()!, model.NewUserName);

            if (res.Success)
            {
                var user = await _userManager.FindByIdAsync(User.Sub()!);
                if (user != null)
                    await _signInManager.RefreshSignInAsync(user);
            }

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            return new ChangeUserNameResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [HttpDelete("disable2Fa")]
    public async Task<DisableMFAResultModel> Disable2FaAsync()
    {
        try
        {
            var res = await _mfaService.DisableMFAAsync(User.Sub()!);

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            return new DisableMFAResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [HttpPost("enable2FA")]
    public async Task<MFAResultModel> Enable2FaAsync([FromBody] EnableMFAModel model)
    {
        try
        {
            var res = await _mfaService.EnableMFAAsync(User.Sub()!, model.Code);

            if (res.Success)
            {
                var user = await _userManager.FindByIdAsync(User.Sub()!);
                if (user != null)
                    await _signInManager.RefreshSignInAsync(user);
            }

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            return new MFAResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [HttpDelete("removePhone")]
    public async Task<ChangePhoneNumberResultModel> RemovePhoneAsync()
    {
        try
        {
            var res = await _phoneNumberService.ChangePhoneNumberAsync(User.Sub()!, "", null);

            if (res.Success)
            {
                var user = await _userManager.FindByIdAsync(User.Sub()!);
                if (user != null)
                    await _signInManager.RefreshSignInAsync(user);
            }

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}",e.Message);

            return new ChangePhoneNumberResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [HttpPost("sendPhoneChangeMessage")]
    public async Task<SendSMSVerificationCodeResultModel> SendPhoneChangeVerificationAsync(
        [FromBody] SendSMSVerificationCodeModel model)
    {
        try
        {
            var res = await _sendSmsVerificationCode.SendCode(User.Sub()!, model.PhoneNumber);

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            return new SendSMSVerificationCodeResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [HttpPost("changePhoneNumber")]
    public async Task<ChangePhoneNumberResultModel> ChangePhoneNumberAsync([FromBody] ChangePhoneNumberModel model)
    {
        try
        {
            var res = await _phoneNumberService.ChangePhoneNumberAsync(User.Sub()!, model.PhoneNumber, model.Token);

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            return new ChangePhoneNumberResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [HttpGet("downloadRecoveryCodes")]
    public async Task<ActionResult> DownloadRecoveryCodesAsync()
    {
        try
        {
            var codes = await _mfaService.GetRecoveryCodes(User.Sub()!);

            var recoveryCodesString = string.Join(",", codes);
            return File(Encoding.Default.GetBytes(recoveryCodesString), "application/text", "recoveryCodes.txt");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            return new EmptyResult();
        }
    }

    [HttpPost("resetRecoveryCodes")]
    public async Task<MFAResultModel> ResetRecoveryCodesAsync()
    {
        try
        {
            return await _mfaService.ResetRecoveryCodesAsync(User.Sub()!);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            return new MFAResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [HttpPost("forgetBrowser")]
    public async Task<bool> ForgetBrowserAsync()
    {
        try
        {
            await _signInManager.ForgetTwoFactorClientAsync();

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            return false;
        }
    }

    [HttpPost("saveProfile")]
    public async Task<SaveProfileResultModel> SaveProfileAsync([FromBody] SaveProfileModel model)
    {
        try
        {
            return await _profileService.SaveProfile(User.Sub()!, model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);

            return new SaveProfileResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }
}