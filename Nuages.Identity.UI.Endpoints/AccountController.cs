using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services;
using Nuages.Identity.Services.Models;
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

    public AccountController(
        IRecaptchaValidator recaptchaValidator, IStringLocalizer stringLocalizer, 
        ILoginService loginService, IForgotPasswordService forgotPasswordService)
    {
        _recaptchaValidator = recaptchaValidator;
        _stringLocalizer = stringLocalizer;
        _loginService = loginService;
        _forgotPasswordService = forgotPasswordService;
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResultModel>> Login([FromBody] LoginModel model)
    {
        if (!await _recaptchaValidator.ValidateAsync(model.RecaptchaToken))
            return new LoginResultModel
            {
                Success = false,
                Result = SignInResult.Failed,
                Reason = FailedLoginReason.RecaptchaError,
                Message = _stringLocalizer["errorMessage:RecaptchaError"]
            };

        return await _loginService.LoginAsync(model);
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
}