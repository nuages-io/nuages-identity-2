using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Login;
using Nuages.Identity.Services.Login.Passwordless;
using Nuages.Identity.Services.Password;
using Nuages.Identity.Services.Register;
using Nuages.Web.Recaptcha;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Controllers;

// ReSharper disable once UnusedType.Global
[ApiController]
[Route("api/[controller]")]

public class AccountController : Controller
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IForgotPasswordService _forgotPasswordService;
    private readonly ILogger<AccountController> _logger;
    private readonly ILoginService _loginService;
    private readonly IPasswordlessService _passwordlessService;
    private readonly IRecaptchaValidator _recaptchaValidator;
    private readonly IRegisterExternalLoginService _registerExternalLoginService;
    private readonly IRegisterService _registerService;
    private readonly IResetPasswordService _resetPasswordService;
    private readonly ISendEmailConfirmationService _sendEmailConfirmationService;
    private readonly ISMSSendCodeService _smsLoginService;
    private readonly IStringLocalizer _stringLocalizer;

    public AccountController(
        IRecaptchaValidator recaptchaValidator, IStringLocalizer stringLocalizer,
        ILoginService loginService, IForgotPasswordService forgotPasswordService,
        IResetPasswordService resetPasswordService,
        ISendEmailConfirmationService sendEmailConfirmationService, IRegisterService registerService,
        IRegisterExternalLoginService registerExternalLoginService,
        IPasswordlessService passwordlessService, ISMSSendCodeService smsLoginService,
        ILogger<AccountController> logger, IHttpContextAccessor contextAccessor)
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
        _smsLoginService = smsLoginService;
        _logger = logger;
        _contextAccessor = contextAccessor;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<LoginResultModel>> LoginAsync([FromBody] LoginModel model,
        [FromHeader(Name = "X-Custom-RecaptchaToken")] string? recaptchaToken)
    {
        try
        {
            var id = Guid.NewGuid().ToString();

            _logger.LogInformation(
                $"Initiate login : ID = {id} {model.UserNameOrEmail} RememberMe = {model.RememberMe} RecaptchaToken = {recaptchaToken}");

            if (!await _recaptchaValidator.ValidateAsync(recaptchaToken))
                return new LoginResultModel
                {
                    Success = false,
                    Result = new SignInResultModel(SignInResult.Failed),
                    Reason = FailedLoginReason.RecaptchaError,
                    Message = _stringLocalizer["errorMessage:RecaptchaError"]
                };

            var res = await _loginService.LoginAsync(model);

            _logger.LogInformation(
                $"Login Result : ID = {id} Success = {res.Success} Result = {res.Result} Resson = {res.Reason} Message = {res.Message}");

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            return new LoginResultModel
            {
                Success = false,
                Message = _stringLocalizer["errorMessage:exception"]
            };
        }
    }

    [HttpPost("login2fa")]
    [AllowAnonymous]
    // ReSharper disable once InconsistentNaming
    public async Task<ActionResult<LoginResultModel>> Login2FAAsync([FromBody] Login2FAModel model,
        [FromHeader(Name = "X-Custom-RecaptchaToken")] string? recaptchaToken)
    {
        try
        {
            var id = Guid.NewGuid().ToString();

            _logger.LogInformation(
                $"Initiate login 2FA : ID = {id} {model.Code} RememberMe = {model.RememberMe} RecaptchaToken = {recaptchaToken}");

            if (!await _recaptchaValidator.ValidateAsync(recaptchaToken))
                return new LoginResultModel
                {
                    Success = false,
                    Result = new SignInResultModel(SignInResult.Failed),
                    Reason = FailedLoginReason.RecaptchaError,
                    Message = _stringLocalizer["errorMessage:RecaptchaError"]
                };

            var res = await _loginService.Login2FAAsync(model);

            _logger.LogInformation(
                $"Login Result : ID = {id} Success = {res.Success} Result = {res.Result} Resson = {res.Reason} Message = {res.Message}");

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            return new LoginResultModel
            {
                Success = false,
                Message = _stringLocalizer["errorMessage:exception"]
            };
        }
    }

    [HttpPost("loginRecoveryCode")]
    [AllowAnonymous]
    // ReSharper disable once InconsistentNaming
    public async Task<ActionResult<LoginResultModel>> LoginRecoveryCodeAsync([FromBody] LoginRecoveryCodeModel model,
        [FromHeader(Name = "X-Custom-RecaptchaToken")] string? recaptchaToken)
    {
        try
        {
            var id = Guid.NewGuid().ToString();

            _logger.LogInformation(
                $"Initiate login Recovery Code : ID = {id} {model.Code} RecaptchaToken = {recaptchaToken}");

            if (!await _recaptchaValidator.ValidateAsync(recaptchaToken))
                return new LoginResultModel
                {
                    Success = false,
                    Result = new SignInResultModel(SignInResult.Failed),
                    Reason = FailedLoginReason.RecaptchaError,
                    Message = _stringLocalizer["errorMessage:RecaptchaError"]
                };

            var res = await _loginService.LoginRecoveryCodeAsync(model);

            _logger.LogInformation(
                $"Login Result : ID = {id} Success = {res.Success} Result = {res.Result} Resson = {res.Reason} Message = {res.Message}");

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            return new LoginResultModel
            {
                Success = false,
                Message = _stringLocalizer["errorMessage:exception"]
            };
        } }


    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<RegisterResultModel> RegisterAsync([FromBody] RegisterModel model,
        [FromHeader(Name = "X-Custom-RecaptchaToken")] string? recaptchaToken)
    {
        try
        {
            
            if (!await _recaptchaValidator.ValidateAsync(recaptchaToken))
                return new RegisterResultModel
                {
                    Success = false
                };

            return await _registerService.Register(model); //SEUL LIGNE QUI CHANGE
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            return new RegisterResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [ExcludeFromCodeCoverage]
    [HttpPost("registerExternalLogin")]
    [AllowAnonymous]
    public async Task<RegisterExternalLoginResultModel> RegisterExternalLoginAsync(
        [FromHeader(Name = "X-Custom-RecaptchaToken")] string? recaptchaToken)
    {
        try
        {
            if (!await _recaptchaValidator.ValidateAsync(recaptchaToken))
                return new RegisterExternalLoginResultModel
                {
                    Success = false
                };

            return await _registerExternalLoginService.Register();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            return new RegisterExternalLoginResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [HttpPost("forgotPassword")]
    [AllowAnonymous]
    public async Task<ForgotPasswordResultModel> ForgotPaswordAsync([FromBody] ForgotPasswordModel model,
        [FromHeader(Name = "X-Custom-RecaptchaToken")] string? recaptchaToken)
    {
        try
        {
            if (!await _recaptchaValidator.ValidateAsync(recaptchaToken))
                return new ForgotPasswordResultModel
                {
                    Success = false
                };

            return await _forgotPasswordService.StartForgotPassword(model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            return new ForgotPasswordResultModel
            {
                Success = false,
                Message = _stringLocalizer["errorMessage:exception"]
            };
        }
    }

    [HttpPost("sendEmailConfirmation")]
    [Authorize(AuthenticationSchemes = NuagesIdentityConstants.EmailNotVerifiedScheme)]
    public async Task<SendEmailConfirmationResultModel> SendEmailConfirmationAsync(
        [FromBody] SendEmailConfirmationModel model,
        [FromHeader(Name = "X-Custom-RecaptchaToken")] string? recaptchaToken)
    {
        try
        {
            if (!await _recaptchaValidator.ValidateAsync(recaptchaToken))
                return new SendEmailConfirmationResultModel
                {
                    Success = false
                };

            model.Email = _contextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Email);

            return await _sendEmailConfirmationService.SendEmailConfirmation(model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            return new SendEmailConfirmationResultModel
            {
                Success = false,
                Message = _stringLocalizer["errorMessage:exception"]
            };
        }
    }

    [HttpPost("resetPassword")]
    [AllowAnonymous]
    public async Task<ResetPasswordResultModel> ResetPasswordAsync([FromBody] ResetPasswordModel model,
        [FromHeader(Name = "X-Custom-RecaptchaToken")] string? recaptchaToken)
    {
        try
        {
            if (!await _recaptchaValidator.ValidateAsync(recaptchaToken))
                return new ResetPasswordResultModel
                {
                    Success = false
                };

            return await _resetPasswordService.Reset(model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            return new ResetPasswordResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }

    [HttpPost("passwordlessLogin")]
    [AllowAnonymous]
    public async Task<StartPasswordlessResultModel> PasswordlessLoginAsync([FromBody] StartPasswordlessModel model,
        [FromHeader(Name = "X-Custom-RecaptchaToken")] string? recaptchaToken)
    {
        try
        {
            if (!await _recaptchaValidator.ValidateAsync(recaptchaToken))
                return new StartPasswordlessResultModel
                {
                    Success = false
                };

            return await _passwordlessService.StartPasswordless(model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            return new StartPasswordlessResultModel
            {
                Success = false,
                Message = _stringLocalizer["errorMessage:exception"]
            };
        }
    }

    [HttpPost("sendSMSCode")]
    [AllowAnonymous]
    public async Task<SendSMSCodeResultModel> SendSMSCodeAsync(
        [FromHeader(Name = "X-Custom-RecaptchaToken")] string? recaptchaToken)
    {
        try
        {
            if (!await _recaptchaValidator.ValidateAsync(recaptchaToken))
                return new SendSMSCodeResultModel
                {
                    Success = false
                };

            return await _smsLoginService.SendCode();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            return new SendSMSCodeResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
    }


    [HttpPost("loginSMS")]
    [AllowAnonymous]
    // ReSharper disable once InconsistentNaming
    public async Task<ActionResult<LoginResultModel>> LoginSMSAsync([FromBody] LoginSMSModel model,
        [FromHeader(Name = "X-Custom-RecaptchaToken")] string? recaptchaToken)
    {
        try
        {
            var id = Guid.NewGuid().ToString();

            _logger.LogInformation($"Initiate login SMS : ID = {id} {model.Code} RecaptchaToken = {recaptchaToken}");

            if (!await _recaptchaValidator.ValidateAsync(recaptchaToken))
                return new LoginResultModel
                {
                    Success = false,
                    Result = new SignInResultModel(SignInResult.Failed),
                    Reason = FailedLoginReason.RecaptchaError,
                    Message = _stringLocalizer["errorMessage:RecaptchaError"]
                };

            var res = await _loginService.LoginSMSAsync(model);

            _logger.LogInformation(
                $"Login Result : ID = {id} Success = {res.Success} Result = {res.Result} Resson = {res.Reason} Message = {res.Message}");

            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            return new LoginResultModel
            {
                Success = false,
                Message = _stringLocalizer["errorMessage:exception"]
            };
        }
    }

   
}