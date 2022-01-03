using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services;
using Nuages.Identity.UI.Endpoints.Models;
using Nuages.Web.Exceptions;
using Nuages.Web.Recaptcha;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Endpoints;

// ReSharper disable once UnusedType.Global
[ApiController]
[Route("api/[controller]")]
public class AccountController
{
    private readonly UserManager<NuagesApplicationUser> _userManager;
    private readonly SignInManager<NuagesApplicationUser> _signInManager;
    private readonly IRecaptchaValidator _recaptchaValidator;
    private readonly IStringLocalizer _stringLocalizer;

    public AccountController(UserManager<NuagesApplicationUser> userManager, SignInManager<NuagesApplicationUser> signInManager, 
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