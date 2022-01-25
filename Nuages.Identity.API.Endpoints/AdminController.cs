using System.Diagnostics.CodeAnalysis;
using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.Manage;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.API.Endpoints;

[ExcludeFromCodeCoverage]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly IChangePasswordService _changePasswordService;
    private readonly IChangePhoneNumberService _changePhoneNumberService;
    private readonly IChangeEmailService _changeEmailService;
    private readonly IMFAService _mfaService;
    private readonly IChangeUserNameService _changeUserNameService;
    private readonly ILogger<AdminController> _logger;
    private readonly IStringLocalizer _localizer;

    public AdminController(IWebHostEnvironment env, IChangePasswordService changePasswordService, 
        IChangePhoneNumberService changePhoneNumberService, IChangeEmailService changeEmailService, IMFAService mfaService, IChangeUserNameService changeUserNameService,
        ILogger<AdminController> logger, IStringLocalizer localizer)
    {
        _env = env;
        _changePasswordService = changePasswordService;
        _changePhoneNumberService = changePhoneNumberService;
        _changeEmailService = changeEmailService;
        _mfaService = mfaService;
        _changeUserNameService = changeUserNameService;
        _logger = logger;
        _localizer = localizer;
    }
    
    [HttpDelete("removeMfa")]
    public async Task<DisableMFAResultModel> AdminRemoveMfaAsync(string userId)
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.AdminRemoveMfaAsync");
        
            return await _mfaService.DisableMFAAsync(userId);
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
    
    [HttpPost("setEmail")]
    public async Task<ChangeEmailResultModel> AdminSetEmailAsync(string userId, string email)
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.AdminSetEmailAsync");
        
            return await _changeEmailService.ChangeEmailAsync(userId, email, null);
        }
        catch (Exception e)
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);
            else
            {
                _logger.LogError(e, e.Message);
            }

            return new ChangeEmailResultModel
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
    
    [HttpPost("setUsername")]
    public async Task<ChangeUserNameResultModel> AdminSetUserNameAsync(string userId, string userName)
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.AdminSetUserNameAsync");
        
            return await _changeUserNameService.ChangeUserNameAsync(userId, userName);
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
    
    [HttpPost("setPhoneNumber")]
    public async Task<ChangePhoneNumberResultModel> AdminSetPhoneNumberAsync(string userId, string phoneNumber)
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.AdminSetPhoneNumberAsync");
        
            return await _changePhoneNumberService.ChangePhoneNumberAsync(userId, phoneNumber, null);
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
                Errors = new List<string> { _localizer["errorMessage:exception"]}
            };
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
        
    }
    
    [HttpPost("setPassword")]
    public async Task<ChangePasswordResultModel> AdminSetPasswordAsync(string userId, [FromBody] ChangePasswordModel model)
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.AdminSetPasswordAsync");

            return await _changePasswordService.AdminChangePasswordAsync(userId, model.NewPassword, model.NewPasswordConfirm, model.MustChangePassword, model.SendByEmail, null);
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
}