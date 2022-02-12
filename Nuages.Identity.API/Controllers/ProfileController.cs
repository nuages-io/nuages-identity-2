using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.Login.Passwordless;
using Nuages.Identity.Services.Manage;
using Nuages.Web;

namespace Nuages.Identity.API.Controllers;

[Route("api/profile")]
[Authorize]
public class ProfileController : Controller
{
    private readonly IChangePasswordService _changePasswordService;
    private readonly IChangePhoneNumberService _changePhoneNumberService;
    private readonly IChangeUserNameService _changeUserNameService;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<ProfileController> _logger;
    private readonly IMFAService _mfaService;
    private readonly IPasswordlessService _passwordlessService;

    public ProfileController(ILogger<ProfileController> logger,
        IChangePasswordService changePasswordService, IChangePhoneNumberService changePhoneNumberService,
        IChangeUserNameService changeUserNameService, IStringLocalizer localizer,
        IPasswordlessService passwordlessService, IMFAService mfaService)
    {
        _logger = logger;
        _changePasswordService = changePasswordService;
        _changePhoneNumberService = changePhoneNumberService;
        _changeUserNameService = changeUserNameService;
        _localizer = localizer;
        _passwordlessService = passwordlessService;
        _mfaService = mfaService;
    }

    [HttpPost("changeUsername")]
    public async Task<ChangeUserNameResultModel> ChangeUserNameAsync([FromBody] ChangeUserNameModel model)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment("AdminController.ChangeUserNameAsync");

            return await _changeUserNameService.ChangeUserNameAsync(User.Sub()!, model.NewUserName);
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new ChangeUserNameResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }


    [HttpPost("changePassword")]
    public async Task<ChangePasswordResultModel> ChangePassword([FromBody] ChangePasswordModel model)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment("AdminController.SetPasswordAsync");

            return await _changePasswordService.ChangePasswordAsync(User.Sub()!, model.CurrentPassword,
                model.NewPassword, model.NewPasswordConfirm);
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new ChangePasswordResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("enableMfa")]
    public async Task<MFAResultModel> EnableMfaAsync([FromBody] EnableMFAModel model)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment("AdminController.EnableMfaAsync");

            return await _mfaService.EnableMFAAsync(User.Sub()!, model.Code);
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new MFAResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpDelete("disableMfa")]
    public async Task<DisableMFAResultModel> DisableMfaAsync()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment("AdminController.DisableMfaAsync");

            return await _mfaService.DisableMFAAsync(User.Sub()!);
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new DisableMFAResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("resetRecoveryCodes")]
    public async Task<MFAResultModel> ResetRecoveryCodesAsync()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment("AdminController.ResetRecoveryCodesAsync");

            return await _mfaService.ResetRecoveryCodesAsync(User.Sub()!);
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new MFAResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpGet("mfaUrl")]
    public async Task<GetMFAUrlResultModel> GetMfaUrlAsync()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment("AdminController.GetMfaUrlAsync");

            return await _mfaService.GetMFAUrlAsync(User.Sub()!);
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new GetMFAUrlResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpGet("passwordlessUrl")]
    public async Task<GetPasswordlessUrlResultModel> GetPasswordlessUrlAsync()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment("AdminController.GetPasswordlessUrlAsync");

            var url = await _passwordlessService.GetPasswordlessUrl(User.Sub()!);

            return new GetPasswordlessUrlResultModel
            {
                Success = true,
                Url = url
            };
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new GetPasswordlessUrlResultModel
            {
                Success = false,
                Message = _localizer["errorMessage:exception"]
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("changePhoneNumber")]
    public async Task<ChangePhoneNumberResultModel> ChangePhoneNumberAsync([FromBody] ChangePhoneNumberModel model)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment("ProfilenController.ChangePhoneNumberAsync");

            return await _changePhoneNumberService.ChangePhoneNumberAsync(User.Sub()!, model.PhoneNumber, model.Token);
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new ChangePhoneNumberResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
}