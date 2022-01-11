using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services;
using Nuages.Web.Recaptcha;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Endpoints;

// ReSharper disable once UnusedType.Global
[ApiController]
[Route("api/[controller]")]
public class AccountController
{
    private readonly IRecaptchaValidator _recaptchaValidator;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly ILoginService _loginService;
    private readonly IForgotPasswordService _forgotPasswordService;
    private readonly IResetPasswordService _resetPasswordService;
    private readonly ILogger<AccountController> _logger;
    private readonly IHostEnvironment _environment;

    public AccountController(
        IRecaptchaValidator recaptchaValidator, IStringLocalizer stringLocalizer, 
        ILoginService loginService, IForgotPasswordService forgotPasswordService, IResetPasswordService resetPasswordService,
        ILogger<AccountController> logger, IHostEnvironment environment)
    {
        _recaptchaValidator = recaptchaValidator;
        _stringLocalizer = stringLocalizer;
        _loginService = loginService;
        _forgotPasswordService = forgotPasswordService;
        _resetPasswordService = resetPasswordService;
        _logger = logger;
        _environment = environment;
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResultModel>> LoginAsync([FromBody] LoginModel model)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.LoginAsync");

            var id = Guid.NewGuid().ToString();
            
            _logger.LogInformation($"Initiate login : ID = {id} {model.UserNameOrEmail} RememberMe = {model.RememberMe} RecaptchaToken = {model.RecaptchaToken}");
        
            if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
                return new LoginResultModel
                {
                    Success = false,
                    Result = SignInResult.Failed,
                    Reason = FailedLoginReason.RecaptchaError,
                    Message = _stringLocalizer["errorMessage:RecaptchaError"]
                };

            var res= await _loginService.LoginAsync(model);
            
            _logger.LogInformation($"Login Result : ID = {id} Success = {res.Success} Result = {res.Result} Resson = {res.Reason} Message = {res.Message}");

            return res;

        }
        catch (Exception e)
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);

            throw;
        }
        finally
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task Register([FromBody] RegisterModel model)
    {
        
    }
    
    [HttpPost("forgotPassword")]
    [AllowAnonymous]
    public async Task<ForgotPasswordResultModel> ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
            return new ForgotPasswordResultModel
            {
                Success = false
            };
        
        await _forgotPasswordService.ForgotPassword(model);
        
        return new ForgotPasswordResultModel
        {
            Success = true
        };
    }
    
    [HttpPost("resetPassword")]
    [AllowAnonymous]
    public async Task<ResetPasswordResultModel> ResetPassword([FromBody] ResetPasswordModel model)
    {
        if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
            return new ResetPasswordResultModel
            {
                Success = false
            };
        
        return await _resetPasswordService.Reset(model);
        
    }
}