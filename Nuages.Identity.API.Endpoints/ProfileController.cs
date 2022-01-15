using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.API.Services;
using Nuages.Identity.Services.AspNetIdentity;
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
    private readonly IMFAService _mfaService;

    
    public ProfileController(IHttpContextAccessor contextAccessor, NuagesUserManager userManager, RoleManager<NuagesApplicationRole> roleManager,
        IWebHostEnvironment env, ILogger<AdminController> logger,
        IChangePasswordService changePasswordService,
        IChangeEmailService changeEmailService,
        IChangePhoneNumberService changePhoneNumberService,
        IChangeUserNameService changeUserNameService,
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
        _mfaService = mfaService;
    }
    
    [HttpPost("changeEmail")]
    public async Task<ChangeEmailResultModel> ChangeEmailAsync([FromBody] ChangeEmailModel model)
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ProfileController.ChangeEmailAsync");

            return await _changeEmailService.ChangeEmailAsync(User.Sub()!, model);
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
    
    [HttpPost("changeUsername")]
    public async Task<ChangeUserNameResultModel> ChangeUserNameAsync([FromBody] ChangeUserNameModel model)
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.ChangeUserNameAsync");

            return await _changeUserNameService.ChangeUserNameAsync(User.Sub()!, model);
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
    
    [HttpPost("changePhoneNumber")]
    public async Task<ChangePhoneNumberResultModel> ChengePhoneNumberAsync([FromBody] ChangePhoneNumberModel model)
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.ChengePhoneNumberAsync");

            return await _changePhoneNumberService.ChangePhoneNumberAsync(User.Sub()!, model);
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

            return await _changePasswordService.ChangePasswordAsync(User.Sub()!, model);
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
}