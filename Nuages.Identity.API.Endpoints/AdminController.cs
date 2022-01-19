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
  
    public AdminController(IWebHostEnvironment env, IChangePasswordService changePasswordService)
    {
        _env = env;
        _changePasswordService = changePasswordService;
    }
    
    [HttpDelete("removeMfa")]
    public async Task<bool> RemoveMfaAsync()
    {
        return await Task.FromResult(true);
    }
    
    [HttpPost("setEmail")]
    public async Task<bool> SetEmailAsync()
    {
        //     try
        //     {
        //         if (!_env.IsDevelopment())
        //             AWSXRayRecorder.Instance.BeginSubsegment("ProfileController.ChangeEmailAsync");
        //
        //         return await _changeEmailService.ChangeEmailAsync(User.Sub()!, model.Email, null);
        //     }
        //     catch (Exception e)
        //     {
        //         if (!_env.IsDevelopment())
        //             AWSXRayRecorder.Instance.AddException(e);
        //
        //         throw;
        //     }
        //     finally
        //     {
        //         if (!_env.IsDevelopment())
        //             AWSXRayRecorder.Instance.EndSubsegment();
        //     }
        
        return await Task.FromResult(true);
    }
    
    [HttpPost("setUsername")]
    public async Task<bool> SetUserNameAsync()
    {
        
        return await Task.FromResult(true);
    }
    
    [HttpPost("setPhoneNumber")]
    public async Task<bool> SetPhoneNumberAsync()
    {
        // try
        // {
        //     if (!_env.IsDevelopment())
        //         AWSXRayRecorder.Instance.BeginSubsegment("AdminController.ChengePhoneNumberAsync");
        //
        //     return await _changePhoneNumberService.ChangePhoneNumberAsync(User.Sub()!, model.PhoneNumber, null);
        // }
        // catch (Exception e)
        // {
        //     if (!_env.IsDevelopment())
        //         AWSXRayRecorder.Instance.AddException(e);
        //
        //     throw;
        // }
        // finally
        // {
        //     if (!_env.IsDevelopment())
        //         AWSXRayRecorder.Instance.EndSubsegment();
        // }
        
        return await Task.FromResult(true);
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