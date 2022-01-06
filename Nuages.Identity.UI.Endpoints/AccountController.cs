using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services;
using Nuages.Identity.UI.Endpoints.Models;
using Nuages.Web.Recaptcha;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Endpoints;

// ReSharper disable once UnusedType.Global
[ApiController]
[Route("api/[controller]")]
public class AccountController
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly IRecaptchaValidator _recaptchaValidator;
    private readonly IStringLocalizer _stringLocalizer;

    public AccountController(NuagesUserManager userManager, NuagesSignInManager signInManager, 
        IRecaptchaValidator recaptchaValidator, IStringLocalizer stringLocalizer)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _recaptchaValidator = recaptchaValidator;
        _stringLocalizer = stringLocalizer;
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
                Reason = FailedLoginReason.RecaptchaError
            };

        var user = await _userManager.FindAsync(model.UserNameOrEmail);
        if (user == null)
        {
            return new LoginResultModel
            {
                Result = SignInResult.Failed
            };
        }
        
        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password,
             true);

        if (result == SignInResult.Success)
        {
            await _signInManager.SignInAsync(user, new AuthenticationProperties{ IsPersistent = model.RememberMe});
                
            return new LoginResultModel
            {
                Success = true
            };
        }

        var message = "login:no_access:" + result;
            
        return new LoginResultModel
        {
            Result = result,
            Message = _stringLocalizer[message],
            Success = false,
            Reason = user.LastFailedLoginReason
        };
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task Register([FromBody] RegisterModel model)
    {
        
    }
}