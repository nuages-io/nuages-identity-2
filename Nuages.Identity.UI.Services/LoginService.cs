using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.UI.Services;

public class LoginService : ILoginService
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly IStringLocalizer _stringLocalizer;

    public LoginService(NuagesUserManager userManager, NuagesSignInManager signInManager, IStringLocalizer stringLocalizer)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _stringLocalizer = stringLocalizer;
    }
    
    public async Task<LoginResultModel> LoginAsync(LoginModel model)
    {
        var user = await _userManager.FindAsync(model.UserNameOrEmail);
        if (user == null)
        {
            return new LoginResultModel
            {
                Result = SignInResult.Failed,
                Message = _stringLocalizer["errorMessage:userNameOrPasswordInvalid"]
            };
        }
        
        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password,
            true);

        if (result.Succeeded)
        {
            result = await _signInManager.CustomSignInOrTwoFactorAsync(user, model.RememberMe);
        }
        
        if (result == SignInResult.Success)
        {
            return new LoginResultModel
            {
                Success = true
            };
        }

        return new LoginResultModel
        {
            Result = result,
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };
    }

    private string GetMessage(FailedLoginReason? failedLoginReason)
    {
        return _stringLocalizer[GetMessageKey(failedLoginReason)];
    }
    private string GetMessageKey(FailedLoginReason? failedLoginReason)
    {
        switch (failedLoginReason)
        {
            case FailedLoginReason.LockedOut:
            case FailedLoginReason.NotWithinDateRange:
            {
                return $"errorMessage:no_access:{failedLoginReason}";
            }
            case FailedLoginReason.UserNameOrPasswordInvalid:
            case FailedLoginReason.RecaptchaError:
            {
                return $"errorMessage:{failedLoginReason}";
            }
            default:
            {
                return "errorMessage:no_access:error";
            }
        }
    }
}



public interface ILoginService
{
    Task<LoginResultModel> LoginAsync(LoginModel model);
}

public class LoginModel
{
    public string UserNameOrEmail { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }

    public string? RecaptchaToken { get; set; } = null!;
}

public class LoginResultModel
{
    public bool Success { get; set; }
    public SignInResult Result { get; set; } = null!;
    public string? Message { get; set; }
    public FailedLoginReason? Reason { get; set; }
}