using System.Security.Claims;
using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Passwordless;
using Nuages.Identity.UI.Services;
using Nuages.Web.Recaptcha;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Endpoints;

// ReSharper disable once UnusedType.Global
[ApiController]
[Route("api/[controller]")]
public class AccountController : Controller
{
    private readonly IRecaptchaValidator _recaptchaValidator;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly ILoginService _loginService;
    private readonly IForgotPasswordService _forgotPasswordService;
    private readonly IResetPasswordService _resetPasswordService;
    private readonly ISendEmailConfirmationService _sendEmailConfirmationService;
    private readonly IRegisterService _registerService;
    private readonly IRegisterExternalLoginService _registerExternalLoginService;
    private readonly IPasswordlessService _passwordlessService;
    private readonly ILogger<AccountController> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IHttpContextAccessor _contextAccessor;

    public AccountController(
        IRecaptchaValidator recaptchaValidator, IStringLocalizer stringLocalizer, 
        ILoginService loginService, IForgotPasswordService forgotPasswordService, IResetPasswordService resetPasswordService,
        ISendEmailConfirmationService sendEmailConfirmationService, IRegisterService registerService, IRegisterExternalLoginService registerExternalLoginService,
        IPasswordlessService passwordlessService,
        ILogger<AccountController> logger, IHostEnvironment environment, IHttpContextAccessor contextAccessor)
    {
        _recaptchaValidator = recaptchaValidator;
        _stringLocalizer = stringLocalizer;
        _loginService = loginService;
        _forgotPasswordService = forgotPasswordService;
        _resetPasswordService = resetPasswordService;
        _sendEmailConfirmationService = sendEmailConfirmationService;
        _registerService = registerService;
        _registerExternalLoginService = registerExternalLoginService;
        _passwordlessService = passwordlessService;
        _logger = logger;
        _environment = environment;
        _contextAccessor = contextAccessor;
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

    [HttpPost("login2fa")]
    [AllowAnonymous]
    // ReSharper disable once InconsistentNaming
    public async Task<ActionResult<LoginResultModel>> Login2FAAsync([FromBody] Login2FAModel model)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.Login2FAAsync");

            var id = Guid.NewGuid().ToString();
            
            _logger.LogInformation($"Initiate login 2FA : ID = {id} {model.Code} RememberMe = {model.RememberMe} RecaptchaToken = {model.RecaptchaToken}");
        
            if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
                return new LoginResultModel
                {
                    Success = false,
                    Result = SignInResult.Failed,
                    Reason = FailedLoginReason.RecaptchaError,
                    Message = _stringLocalizer["errorMessage:RecaptchaError"]
                };

            var res= await _loginService.Login2FAAsync(model);
            
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

    [HttpPost("loginRecoveryCOde")]
    [AllowAnonymous]
    // ReSharper disable once InconsistentNaming
    public async Task<ActionResult<LoginResultModel>> LoginRecoveryCodeAsync([FromBody] LoginRecoveryCodeModel model)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.LoginRecoveryCodeAsync");

            var id = Guid.NewGuid().ToString();
            
            _logger.LogInformation($"Initiate login Recovery Code : ID = {id} {model.Code} RecaptchaToken = {model.RecaptchaToken}");
        
            if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
                return new LoginResultModel
                {
                    Success = false,
                    Result = SignInResult.Failed,
                    Reason = FailedLoginReason.RecaptchaError,
                    Message = _stringLocalizer["errorMessage:RecaptchaError"]
                };

            var res= await _loginService.LoginRecoveryCodeAsync(model);
            
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
                return new RegisterResultModel
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
                return new RegisterExternalLoginResultModel
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
    public async Task<ForgotPasswordResultModel> PasswordLoginAsync([FromBody] ForgotPasswordModel model)
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
        
            model.Email = _contextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Email);
            
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

    [HttpPost("passwordlessLogin")]
    [AllowAnonymous]
    public async Task<StartPasswordlessResultModel> PasswordLoginAsync([FromBody] StartPasswordlessModel model)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.PasswordLoginAsync");
            
            if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
                return new StartPasswordlessResultModel
                {
                    Success = false
                };
        
            return await _passwordlessService.StartPasswordless(model);
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
    
    [HttpPost("sendSMSCode")]
    [AllowAnonymous]
    public async Task<StartPasswordlessResultModel> SendSMSCodeAsync([FromBody] StartPasswordlessModel model)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AccountController.SendSMSCodeAsync");
            
            if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
                return new StartPasswordlessResultModel
                {
                    Success = false
                };
        
            return await _passwordlessService.StartPasswordless(model);
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