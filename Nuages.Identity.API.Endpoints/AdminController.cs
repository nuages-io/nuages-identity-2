using System.Diagnostics.CodeAnalysis;
using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.Services.Manage;
using Nuages.Web;

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

    public AdminController(IWebHostEnvironment env, IChangePasswordService changePasswordService, 
        IChangePhoneNumberService changePhoneNumberService, IChangeEmailService changeEmailService, IMFAService mfaService, IChangeUserNameService changeUserNameService)
    {
        _env = env;
        _changePasswordService = changePasswordService;
        _changePhoneNumberService = changePhoneNumberService;
        _changeEmailService = changeEmailService;
        _mfaService = mfaService;
        _changeUserNameService = changeUserNameService;
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
        
            throw;
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
        
            throw;
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
        
            throw;
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
        
            throw;
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
        
    }
    
    [HttpPost("setPassword")]
    public async Task<ChangePasswordResultModel> AdminSetPasswordAsync([FromBody] ChangePasswordModel model)
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.AdminSetPasswordAsync");

            return await _changePasswordService.AdminChangePasswordAsync(User.Sub()!, model.NewPassword, model.NewPasswordConfirm, model.MustChangePassword, model.SendByEmail, null);
        }
        catch (Exception e)
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);

            throw;
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
}