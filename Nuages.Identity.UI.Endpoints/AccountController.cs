using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MongoDB.Bson;
using Nuages.Identity.Services;
using Nuages.Identity.Services.AspNetIdentity;
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
    private readonly ISendEmailConfirmationService _sendEmailConfirmationService;
    private readonly IRegisterService _registerService;
    private readonly IRegisterExternalLoginService _registerExternalLoginService;
    private readonly ILogger<AccountController> _logger;
    private readonly IHostEnvironment _environment;

    public AccountController(
        IRecaptchaValidator recaptchaValidator, IStringLocalizer stringLocalizer, 
        ILoginService loginService, IForgotPasswordService forgotPasswordService, IResetPasswordService resetPasswordService,
        ISendEmailConfirmationService sendEmailConfirmationService, IRegisterService registerService, IRegisterExternalLoginService registerExternalLoginService,
        ILogger<AccountController> logger, IHostEnvironment environment)
    {
        _recaptchaValidator = recaptchaValidator;
        _stringLocalizer = stringLocalizer;
        _loginService = loginService;
        _forgotPasswordService = forgotPasswordService;
        _resetPasswordService = resetPasswordService;
        _sendEmailConfirmationService = sendEmailConfirmationService;
        _registerService = registerService;
        _registerExternalLoginService = registerExternalLoginService;
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
    public async Task<RegisterResultModel> Register([FromBody] RegisterModel model)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.ForgotPasswordAsync");
            
            if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
                return new RegisterResultModel()
                {
                    Success = false
                };
        
            return await _registerService.Register(model);
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
    
    [HttpPost("registerExternalLogin")]
    [AllowAnonymous]
    public async Task<RegisterExternalLoginResultModel> RegisterExternalLogin([FromBody] RegisterExternalLoginModel model)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.ForgotPasswordAsync");
            
            if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
                return new RegisterExternalLoginResultModel()
                {
                    Success = false
                };
        
            return await _registerExternalLoginService.Register(model);
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
    
    [HttpPost("forgotPassword")]
    [AllowAnonymous]
    public async Task<ForgotPasswordResultModel> ForgotPasswordAsync([FromBody] ForgotPasswordModel model)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.ForgotPasswordAsync");
            
            if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
                return new ForgotPasswordResultModel
                {
                    Success = false
                };
        
            return await _forgotPasswordService.ForgotPassword(model);
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
    
    [HttpPost("sendEmailConfirmation")]
    [Authorize(AuthenticationSchemes = NuagesIdentityConstants.EmailNotVerifiedScheme)]
    public async Task<SendEmailConfirmationResultModel> SendEmailConfirmationAsync([FromBody] SendEmailConfirmationModel model)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.SendEmailConfirmationAsync");
            
            if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
                return new SendEmailConfirmationResultModel
                {
                    Success = false
                };
        
            return await _sendEmailConfirmationService.SendEmailConfirmation(model);

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
    
    [HttpPost("resetPassword")]
    [AllowAnonymous]
    public async Task<ResetPasswordResultModel> ResetPasswordAsync([FromBody] ResetPasswordModel model)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.ResetPasswordAsync");
            
            if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
                return new ResetPasswordResultModel
                {
                    Success = false
                };
        
            return await _resetPasswordService.Reset(model);
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
}