using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services;
using Nuages.Identity.UI.Models;
using Nuages.Web.Exceptions;
using Nuages.Web.Recaptcha;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Nuages.Identity.UI.Controllers;

// ReSharper disable once UnusedType.Global
public class AccountController
{
    private readonly NuagesUserManager<NuagesApplicationUser> _userManager;
    private readonly NuagesSignInManager<NuagesApplicationUser> _signInManager;
    private readonly IRecaptchaValidator _recaptchaValidator;
    private readonly IStringLocalizer _stringLocalizer;

    public AccountController(NuagesUserManager<NuagesApplicationUser> userManager, NuagesSignInManager<NuagesApplicationUser> signInManager, 
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
                Reason = SignInFailedReason.RecaptchaError
            };
            
        var result = await _signInManager.PasswordSignInAsync(model.UserNameOrEmail, model.Password,
            model.RememberMe, true);

        if (result == SignInResult.Success)
        {
            var user = await _userManager.FindByNameAsync(model.UserNameOrEmail);
            if (user == null)
                throw new NotAuthorizedException();
                    
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
            Success = false
        };
    }
}