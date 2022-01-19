using System.Diagnostics.CodeAnalysis;

using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Login.Passwordless;
using Nuages.Identity.Services.Manage;
using Nuages.Web;

namespace Nuages.Identity.API.Endpoints;

[ExcludeFromCodeCoverage]

[Route("api/profile")]
[Authorize]
public class ProfileController : Controller
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly NuagesUserManager _userManager;
    private readonly RoleManager<NuagesApplicationRole> _roleManager;

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AdminController> _logger;
    private readonly IChangePasswordService _changePasswordService;
    private readonly IChangeEmailService _changeEmailService;
    private readonly IChangePhoneNumberService _changePhoneNumberService;
    private readonly IChangeUserNameService _changeUserNameService;
    private readonly IPasswordlessService _passwordlessService;
    private readonly IMFAService _mfaService;

    
    public ProfileController(IHttpContextAccessor contextAccessor, NuagesUserManager userManager, RoleManager<NuagesApplicationRole> roleManager,
        IWebHostEnvironment env, ILogger<AdminController> logger,
       
        IChangeEmailService changeEmailService,
        
        IChangePasswordService changePasswordService,
        IChangePhoneNumberService changePhoneNumberService,
        IChangeUserNameService changeUserNameService,
        
        IPasswordlessService passwordlessService,
        IMFAService mfaService)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _roleManager = roleManager;
        
        _env = env;
        _logger = logger;
        _changePasswordService = changePasswordService;
        _changeEmailService = changeEmailService;
        _changePhoneNumberService = changePhoneNumberService;
        _changeUserNameService = changeUserNameService;
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

            throw;
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

            throw;
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

            return await _mfaService.EnableMFAAsync(User.Sub()!, model);
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

            throw;
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

            throw;
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

            throw;
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

            throw;
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
}