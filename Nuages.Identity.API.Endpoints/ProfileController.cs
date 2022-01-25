using System.Diagnostics.CodeAnalysis;

using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.Login.Passwordless;
using Nuages.Identity.Services.Manage;
using Nuages.Web;

namespace Nuages.Identity.API.Endpoints;

[ExcludeFromCodeCoverage]

[Route("api/profile")]
[Authorize]
public class ProfileController : Controller
{

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ProfileController> _logger;
    private readonly IChangePasswordService _changePasswordService;
    private readonly IChangePhoneNumberService _changePhoneNumberService;
    private readonly IChangeUserNameService _changeUserNameService;
    private readonly IStringLocalizer _localizer;
    private readonly IPasswordlessService _passwordlessService;
    private readonly IMFAService _mfaService;

    
    public ProfileController(IWebHostEnvironment env, ILogger<ProfileController> logger,
       
        IChangePasswordService changePasswordService,
        IChangePhoneNumberService changePhoneNumberService,
        IChangeUserNameService changeUserNameService,
        IStringLocalizer localizer,
        IPasswordlessService passwordlessService,
        IMFAService mfaService)
    {
        _env = env;
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
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.ChangeUserNameAsync");

            return await _changeUserNameService.ChangeUserNameAsync(User.Sub()!, model.NewUserName);
        }
        catch (Exception e)
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, e.Message);
            }

            return new ChangeUserNameResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"]}
            };
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
    
    
    [HttpPost("changePassword")]
    public async Task<ChangePasswordResultModel> ChangePassword([FromBody] ChangePasswordModel model)
    { 
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.SetPasswordAsync");

            return await _changePasswordService.ChangePasswordAsync(User.Sub()!, model.CurrentPassword, model.NewPassword, model.NewPasswordConfirm);
        }
        catch (Exception e)
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, e.Message);
            }

            return new ChangePasswordResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"]}
            };
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
        
    }
    
    [HttpPost("enableMfa")]
    public async Task<MFAResultModel> EnableMfaAsync([FromBody] EnableMFAModel model)
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.EnableMfaAsync");

            return await _mfaService.EnableMFAAsync(User.Sub()!, model.Code);
        }
        catch (Exception e)
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, e.Message);
            }

            return new MFAResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"]}
            };
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
    
    [HttpDelete("disableMfa")]
    public async Task<DisableMFAResultModel> DisableMfaAsync()
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.DisableMfaAsync");

            return await _mfaService.DisableMFAAsync(User.Sub()!);
        }
        catch (Exception e)
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, e.Message);
            }

            return new DisableMFAResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"]}
            };
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
    
    [HttpPost("resetRecoveryCodes")]
    public async Task<MFAResultModel> ResetRecoveryCodesAsync()
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.ResetRecoveryCodesAsync");

            return await _mfaService.ResetRecoveryCodesAsync(User.Sub()!);
        }
        catch (Exception e)
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, e.Message);
            }

            return new MFAResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"]}
            };
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
    
    [HttpGet("mfaUrl")]
    public async Task<GetMFAUrlResultModel> GetMfaUrlAsync()
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.GetMfaUrlAsync");

            return await _mfaService.GetMFAUrlAsync(User.Sub()!);
        }
        catch (Exception e)
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, e.Message);
            }

            return new GetMFAUrlResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["errorMessage:exception"]}
            };
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
    
    [HttpGet("passwordlessUrl")]
    public async Task<GetPasswordlessUrlResultModel> GetPasswordlessUrlAsync()
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.GetPasswordlessUrlAsync");

            return await _passwordlessService.GetPasswordlessUrl(User.Sub()!);
        }
        catch (Exception e)
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, e.Message);
            }

            return new GetPasswordlessUrlResultModel
            {
                Success = false,
                Message = _localizer["errorMessage:exception"]
            };
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
    
    [HttpPost("changePhoneNumber")]
    public async Task<ChangePhoneNumberResultModel> ChangePhoneNumberAsync([FromBody] ChangePhoneNumberModel model)
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ProfilenController.ChangePhoneNumberAsync");
        
            return await _changePhoneNumberService.ChangePhoneNumberAsync(User.Sub()!, model.PhoneNumber, model.Token);
        }
        catch (Exception e)
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, e.Message);
            }

            return new ChangePhoneNumberResultModel
            {
                Success = false,
                Errors =new List<string> { _localizer["errorMessage:exception"]}
            };
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
}