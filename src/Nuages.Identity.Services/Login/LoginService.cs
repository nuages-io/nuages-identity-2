using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Web.Exceptions;

// ReSharper disable InconsistentNaming

namespace Nuages.Identity.Services.Login;

public class LoginService : ILoginService
{
    private readonly IMessageService _messageService;
    private readonly ILogger<LoginService> _logger;
    private readonly IIdentityEventBus _identityEventBus;
    private readonly NuagesSignInManager _signInManager;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly NuagesUserManager _userManager;

    public LoginService(NuagesUserManager userManager, NuagesSignInManager signInManager,
        IStringLocalizer stringLocalizer, IMessageService messageService, ILogger<LoginService> logger, IIdentityEventBus identityEventBus)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _stringLocalizer = stringLocalizer;
        _messageService = messageService;
        _logger = logger;
        _identityEventBus = identityEventBus;
    }

    public async Task<LoginResultModel> LoginAsync(LoginModel model)
    {
        var user = await _userManager.FindAsync(model.UserNameOrEmail);
        if (user == null)
            return new LoginResultModel
            {
                Result = new SignInResultModel(SignInResult.Failed),
                Reason = FailedLoginReason.UserNameOrPasswordInvalid,
                Message = _stringLocalizer["errorMessage:userNameOrPasswordInvalid"]
            };

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
                    await _identityEventBus.PutEvent(IdentityEvents.LockingOutUser, user);
                        
                    _messageService.SendEmailUsingTemplate(user.Email!, "Login_LockedOut", new Dictionary<string, string>
                    {
                        { "Minutes", _userManager.Options.Lockout.DefaultLockoutTimeSpan.Minutes.ToString() }
                    });

                    user.LockoutMessageSent = true;
                    var updateRes = await _userManager.UpdateAsync(user);
                    if (!updateRes.Succeeded)
                    {
                        _logger.LogError(updateRes.Errors.First().Description);
                    }
                }
               
                await _identityEventBus.PutEvent(IdentityEvents.FailedLoginUserIsLockedOut, user);
            }
            else
            {
                if (user.LastFailedLoginReason == FailedLoginReason.EmailNotConfirmed)
                {
                    await _identityEventBus.PutEvent(IdentityEvents.FailedLoginUserIsNotConfirmed, user);
                    await _signInManager.SignInEmailNotVerified(user);
                }
            }
        }

        if (result == SignInResult.Success)
        {
            await _identityEventBus.PutEvent(IdentityEvents.Login, user);
            
            return new LoginResultModel
            {
                Success = true
            };
        }
        
        var loginResultModel = new LoginResultModel
        {
            Result = new SignInResultModel(result),
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };
        
        await _identityEventBus.PutEvent(IdentityEvents.LoginFailed, new { User = user, Result = loginResultModel});

        return loginResultModel;
    }

    public async Task<LoginResultModel> Login2FAAsync(Login2FAModel model)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null) 
            throw new NotFoundException("UserNotFound");

        var result =
            await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe, model.RememberMachine);

        //Reload to avoind concurrency error
        user = await _userManager.FindByIdAsync(user.Id);
        
        if (user == null) 
            throw new NotFoundException("UserNotFound");
        
        if (result == SignInResult.Success)
        {
            user.PreferredMfaMethod = "Authenticator";
            var updateRes = await _userManager.UpdateAsync(user);
            if (!updateRes.Succeeded)
            {
                _logger.LogError(updateRes.Errors.First().Description);
            }
            
            await _identityEventBus.PutEvent(IdentityEvents.Login2FASuccess, user);
            
            return new LoginResultModel
            {
                Success = true
            };
        }
          
        user.LastFailedLoginReason = FailedLoginReason.FailedMfa;
        var updateRes2 = await _userManager.UpdateAsync(user);
        if (!updateRes2.Succeeded)
        {
            _logger.LogError(updateRes2.Errors.First().Description);
        }
        
        var resultModel = new LoginResultModel
        {
            Result = new SignInResultModel(result),
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };

        await _identityEventBus.PutEvent(IdentityEvents.Login2FAFailed, new { User = user, Result = resultModel});
            
        return resultModel;
    }

    public async Task<LoginResultModel> LoginRecoveryCodeAsync(LoginRecoveryCodeModel model)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null) throw new NotFoundException("UserNotFound");

        var recoveryCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        if (result == SignInResult.Success)
        {
            await _identityEventBus.PutEvent(IdentityEvents.LoginRecoveryCodeSuccess, user);
            
            return new LoginResultModel
            {
                Success = true
            };
        }
          

        user.LastFailedLoginReason = FailedLoginReason.FailedRecoveryCode;
        
        var updateRes = await _userManager.UpdateAsync(user);
        if (!updateRes.Succeeded)
        {
            _logger.LogError(updateRes.Errors.First().Description);
        }
        
        var resultModel = new LoginResultModel
        {
            Result = new SignInResultModel(result),
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };

        await _identityEventBus.PutEvent(IdentityEvents.LoginRecoveryCodeFailed, new { User = user, resultModel});
        
        return resultModel;
    }

    public async Task<LoginResultModel> LoginSMSAsync(LoginSMSModel model)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null) throw new NotFoundException("UserNotFound");

        var code = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorSignInAsync("Phone", code, false, false);

        if (result == SignInResult.Success)
        {
            await _identityEventBus.PutEvent(IdentityEvents.LoginSMSSuccess, user);
            
            return new LoginResultModel
            {
                Success = true
            };
            
        }

        user.LastFailedLoginReason = FailedLoginReason.FailedSms;
        
        var updateRes = await _userManager.UpdateAsync(user);
        if (!updateRes.Succeeded)
        {
            _logger.LogError(updateRes.Errors.First().Description);
        }
        
        var resultModel = new LoginResultModel
        {
            Result = new SignInResultModel(result),
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };

        await _identityEventBus.PutEvent(IdentityEvents.LoginSMSFailed, new { User = user, Result = resultModel});
        
        return resultModel;
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
    // ReSharper disable once UnusedMember.Global
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

    // ReSharper disable once MemberCanBePrivate.Global
    public bool Succeeded { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public bool IsLockedOut { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public bool IsNotAllowed { get; set; }
    public bool RequiresTwoFactor { get; set; }
}