
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

using Nuages.AspNetIdentity.Core;
using Nuages.Identity.Services.Email;
using Nuages.Web.Exceptions;
// ReSharper disable InconsistentNaming

namespace Nuages.Identity.Services.Login;

public class LoginService : ILoginService
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly IMessageService _messageService;

    public LoginService(NuagesUserManager userManager, NuagesSignInManager signInManager, IStringLocalizer stringLocalizer, IMessageService messageService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _stringLocalizer = stringLocalizer;
        _messageService = messageService;
    }
    
    public async Task<LoginResultModel> LoginAsync(LoginModel model)
    {
        var user = await _userManager.FindAsync(model.UserNameOrEmail);
        if (user == null)
        {
            return new LoginResultModel
            {
                Result = new SignInResultModel(SignInResult.Failed),
                Reason = FailedLoginReason.UserNameOrPasswordInvalid,
                Message = _stringLocalizer["errorMessage:userNameOrPasswordInvalid"]
            };
        }
        
        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password,
            true);

        if (result.Succeeded)
        {
            result = await _signInManager.CustomSignInOrTwoFactorAsync(user, model.RememberMe);
        }
        else
        {
            if (result.IsLockedOut)
            {
                if (!user.LockoutMessageSent)
                {
                    _messageService.SendEmailUsingTemplate(user.Email, "Login_LockedOut", new Dictionary<string, string>
                    {
                        { "Minutes", _userManager.Options.Lockout.DefaultLockoutTimeSpan.Minutes.ToString() }
                    });

                    user.LockoutMessageSent = true;
                    await _userManager.UpdateAsync(user);
                }
                
            }
            else
            {
                if (user.LastFailedLoginReason == FailedLoginReason.EmailNotConfirmed)
                {
                    await _signInManager.SignInEmailNotVerified(user);
                }
            }
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
            Result = new SignInResultModel(result),
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };
    }

    public async Task<LoginResultModel> Login2FAAsync(Login2FAModel model)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new NotFoundException("UserNotFound");
        }

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe, model.RememberMachine);

        if (result == SignInResult.Success)
        {
            return new LoginResultModel
            {
                Success = true
            };
        }

        user.LastFailedLoginReason = FailedLoginReason.FailedMfa;
        await _userManager.UpdateAsync(user);
        
        return new LoginResultModel
        {
            Result = new SignInResultModel(result),
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };
    }
    
    public async Task<LoginResultModel> LoginRecoveryCodeAsync(LoginRecoveryCodeModel model)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new NotFoundException("UserNotFound");
        }

        var recoveryCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        if (result == SignInResult.Success)
        {
            return new LoginResultModel
            {
                Success = true
            };
        }

        user.LastFailedLoginReason = FailedLoginReason.FailedRecoveryCode;
        await _userManager.UpdateAsync(user);

        return new LoginResultModel
        {
            Result = new SignInResultModel(result),
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };
    }
    
    public async Task<LoginResultModel> LoginSMSAsync(LoginSMSModel model)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new NotFoundException("UserNotFound");
        }

        var code = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorSignInAsync("Phone", code, false, false);

        if (result == SignInResult.Success)
        {
            return new LoginResultModel
            {
                Success = true
            };
        }

        user.LastFailedLoginReason = FailedLoginReason.FailedSms;
        await _userManager.UpdateAsync(user);

        return new LoginResultModel
        {
            Result = new SignInResultModel(result),
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };
    }

    
    private string? GetMessage(FailedLoginReason? failedLoginReason)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (failedLoginReason == null)
            return null;
        
        return _stringLocalizer[GetMessageKey(failedLoginReason)];
    }
    
    
    public static string GetMessageKey(FailedLoginReason? failedLoginReason)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (failedLoginReason)
        {
            case FailedLoginReason.LockedOut:
            case FailedLoginReason.NotWithinDateRange:
            case FailedLoginReason.EmailNotConfirmed:
            case FailedLoginReason.AccountNotConfirmed:
            {
                return $"errorMessage:no_access:{failedLoginReason}";
            }
            case FailedLoginReason.UserNameOrPasswordInvalid:
            case FailedLoginReason.RecaptchaError:
            {
                return $"errorMessage:{failedLoginReason}";
            }
            
            // case FailedLoginReason.PasswordMustBeChanged:
            // case FailedLoginReason.PhoneNotConfirmed:
            // case FailedLoginReason.PasswordExpired:
            // {
            //     throw new NotSupportedException("ValueNotSupportedHere");
            // }
            case null:
            {
                return "";
            }
            default:
            {
                return "errorMessage.no_access.error";
            }
        }
    }
}



public interface ILoginService
{
    Task<LoginResultModel> LoginAsync(LoginModel model);
    Task<LoginResultModel> Login2FAAsync(Login2FAModel model);
    Task<LoginResultModel> LoginRecoveryCodeAsync(LoginRecoveryCodeModel model);
    Task<LoginResultModel> LoginSMSAsync(LoginSMSModel model);
}

// ReSharper disable once ClassNeverInstantiated.Global
public class LoginModel
{
    public string UserNameOrEmail { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }

}

public class Login2FAModel
{
    public string Code { get; set; } = null!;

    public bool RememberMe { get; set; }
    public bool RememberMachine { get; set; }
    
}

public class LoginRecoveryCodeModel
{
    public string Code { get; set; } = null!;
    
}

public class LoginSMSModel
{
    public string Code { get; set; } = null!;
    
}

public class LoginResultModel
{
    public bool Success { get; set; }
    public SignInResultModel Result { get; set; } = null!;
    public string? Message { get; set; }
    
    public FailedLoginReason Reason { get; set; }
}

public class SignInResultModel
{
    public SignInResultModel()
    {
        
    }
    public SignInResultModel(SignInResult result)
    {
        Succeeded = result.Succeeded;
        IsLockedOut = result.IsLockedOut;
        IsNotAllowed = result.IsNotAllowed;
        RequiresTwoFactor = result.RequiresTwoFactor;
    }

    public bool Succeeded { get; set; }
    public bool IsLockedOut { get; set; }
    public bool IsNotAllowed { get; set; }
    public bool RequiresTwoFactor { get; set; }
}