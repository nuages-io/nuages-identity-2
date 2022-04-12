using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

// ReSharper disable ContextualLoggerProblem

namespace Nuages.Identity.Services.AspNetIdentity;

// ReSharper disable once ClassNeverInstantiated.Global
public class NuagesSignInManager : SignInManager<NuagesApplicationUser<string>>
{
    private readonly IUserConfirmation<NuagesApplicationUser<string>> _confirmation;
    private readonly NuagesIdentityOptions _nuagesIdentityOptions;

    // ReSharper disable once MemberCanBeProtected.Global
    public NuagesSignInManager(UserManager<NuagesApplicationUser<string>> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<NuagesApplicationUser<string>> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<NuagesApplicationUser<string>>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<NuagesApplicationUser<string>> confirmation,
        IOptions<NuagesIdentityOptions> nuagesIdentityOptions) :
        base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _confirmation = confirmation;
        _nuagesIdentityOptions = nuagesIdentityOptions.Value;
    }

    public override async Task<bool> CanSignInAsync(NuagesApplicationUser<string> user)
    {
        if (!await CheckStartEndAsync(user))
            return false;

        if (!await CheckEmailAsync(user))
            return false;

        if (!await CheckPhoneNumberAsync(user))
            return false;

        return await CheckAccountAsync(user);
    }

    public override async Task<SignInResult> CheckPasswordSignInAsync(NuagesApplicationUser<string> user,
        string password, bool lockoutOnFailure)
    {
        var res = await base.CheckPasswordSignInAsync(user, password, lockoutOnFailure);

        if (res.Succeeded)
        {
            if (!await CheckPasswordStatusAsync(user))
                return SignInResult.NotAllowed;
            
        }
        else
        {
            if (!res.IsLockedOut && !res.IsNotAllowed && !res.RequiresTwoFactor)
            {
                user.LastFailedLoginReason = FailedLoginReason.UserNameOrPasswordInvalid;
                var updateRes = await UserManager.UpdateAsync(user);
                if (!updateRes.Succeeded)
                {
                    Logger.LogError(updateRes.Errors.First().Description);
                    return SignInResult.Failed;
                }
            }
        }

        return res;
    }

    public async Task<bool> CheckAccountAsync(NuagesApplicationUser<string> user)
    {
        if (Options.SignIn.RequireConfirmedAccount && !await _confirmation.IsConfirmedAsync(UserManager, user))
        {
            user.LastFailedLoginReason = FailedLoginReason.AccountNotConfirmed;

            var updateRes = await UserManager.UpdateAsync(user);
            if (!updateRes.Succeeded)
            {
                Logger.LogError(updateRes.Errors.First().Description);
                return false;
            }

            return false;
        }

        return true;
    }


    private static ClaimsPrincipal StoreAuthInfo(string authType, string userId, string email, string? code = null)
    {
        var identity = new ClaimsIdentity(authType);
        identity.AddClaim(new Claim(ClaimTypes.Name, userId));
        identity.AddClaim(new Claim(ClaimTypes.Email, email));
        if (!string.IsNullOrEmpty(code))
            identity.AddClaim(new Claim(ClaimTypes.UserData, code));

        return new ClaimsPrincipal(identity);
    }

    public async Task<bool> CheckPhoneNumberAsync(NuagesApplicationUser<string> user)
    {
        if (Options.SignIn.RequireConfirmedPhoneNumber && !await UserManager.IsPhoneNumberConfirmedAsync(user))
        {
            user.LastFailedLoginReason = FailedLoginReason.PhoneNotConfirmed;

            var updateRes = await UserManager.UpdateAsync(user);
            if (!updateRes.Succeeded)
            {
                Logger.LogError(updateRes.Errors.First().Description);
            }
            
            return false;
        }

        return true;
    }

    private async Task<bool> CheckEmailAsync(NuagesApplicationUser<string> user)
    {
        if (Options.SignIn.RequireConfirmedEmail && !await UserManager.IsEmailConfirmedAsync(user))
        {
            user.LastFailedLoginReason = FailedLoginReason.EmailNotConfirmed;

            var updateRes = await UserManager.UpdateAsync(user);
            if (!updateRes.Succeeded)
            {
                Logger.LogError(updateRes.Errors.First().Description);
            }
            
            return false;
        }

        return true;
    }


    private async Task<bool> CheckPasswordStatusAsync(NuagesApplicationUser<string> user)
    {
        if (user.UserMustChangePassword)
        {
            user.LastFailedLoginReason = FailedLoginReason.PasswordMustBeChanged;

            var updateRes = await UserManager.UpdateAsync(user);
            if (!updateRes.Succeeded)
            {
                Logger.LogError(updateRes.Errors.First().Description);
                return  false;
            }
            
            var newCode = UserManager.GeneratePasswordResetTokenAsync(user).Result;
            newCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(newCode));

            await Context.SignInAsync(NuagesIdentityConstants.PasswordExpiredScheme,
                StoreAuthInfo("PasswordExpired", user.Id, user.Email, newCode));

            return false;
        }

        if (_nuagesIdentityOptions.EnableAutoPasswordExpiration)
        {
            if (!user.LastPasswordChangedDate.HasValue) throw new NotSupportedException("PasswordWasNeverSet");

            if (user.EnableAutoExpirePassword)
                if (DateTimeOffset.UtcNow >
                    user.LastPasswordChangedDate.Value.AddDays(_nuagesIdentityOptions.AutoExpirePasswordDelayInDays))
                {
                    user.LastFailedLoginReason = FailedLoginReason.PasswordExpired;

                    var updateRes =await UserManager.UpdateAsync(user);
                    if (!updateRes.Succeeded)
                    {
                        Logger.LogError(updateRes.Errors.First().Description);
                        return false;
                    }
                    
                    var newCode = UserManager.GeneratePasswordResetTokenAsync(user).Result;
                    newCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(newCode));

                    await Context.SignInAsync(NuagesIdentityConstants.PasswordExpiredScheme,
                        StoreAuthInfo("PasswordExpired", user.Id, user.Email, newCode));

                    return false;
                }
        }

        
        return true;
    }

    public async Task<bool> CheckStartEndAsync(NuagesApplicationUser<string> user)
    {
        if (user.ValidFrom.HasValue)
            if (user.ValidFrom > DateTimeOffset.UtcNow)
            {
                user.LastFailedLoginReason = FailedLoginReason.NotWithinDateRange;

                var updateRes = await UserManager.UpdateAsync(user);
                if (!updateRes.Succeeded)
                {
                    Logger.LogError(updateRes.Errors.First().Description);
                }
                
                return false;
            }

        if (user.ValidTo.HasValue)
            if (DateTime.UtcNow > user.ValidTo)
            {
                user.LastFailedLoginReason = FailedLoginReason.NotWithinDateRange;

                var updateRes = await UserManager.UpdateAsync(user);
                if (!updateRes.Succeeded)
                {
                    Logger.LogError(updateRes.Errors.First().Description);
                    return false;
                }
                
                return false;
            }

        return true;
    }


    public override async Task SignInWithClaimsAsync(NuagesApplicationUser<string> user,
        AuthenticationProperties authenticationProperties,
        IEnumerable<Claim> additionalClaims)
    {
        user.LastLogin = DateTime.UtcNow;
        user.LoginCount++;
        user.LockoutEnd = null;
        user.LockoutMessageSent = false;
        user.AccessFailedCount = 0;

        var updateRes = await UserManager.UpdateAsync(user);
        if (!updateRes.Succeeded)
        {
            Logger.LogError(updateRes.Errors.First().Description);
            return;
        }
        
        await base.SignInWithClaimsAsync(user, authenticationProperties, additionalClaims);
    }

    protected override async Task<SignInResult> LockedOut(NuagesApplicationUser<string> user)
    {
        user.LastFailedLoginReason = FailedLoginReason.LockedOut;

        var updateRes = await UserManager.UpdateAsync(user);
        if (!updateRes.Succeeded)
        {
            Logger.LogError(updateRes.Errors.First().Description);
            return SignInResult.Failed;
        }
        
        return await base.LockedOut(user);
    }

    public Task<SignInResult> CustomSignInOrTwoFactorAsync(NuagesApplicationUser<string> user, bool isPersistent,
        string? loginProvider = null, bool bypassTwoFactor = false)
    {
        return base.SignInOrTwoFactorAsync(user, isPersistent, loginProvider, bypassTwoFactor);
    }

    public async Task SignInEmailNotVerified(NuagesApplicationUser<string> user)
    {
        await Context.SignInAsync(NuagesIdentityConstants.EmailNotVerifiedScheme,
            StoreAuthInfo("EmailNotConfirmed", user.Id, user.Email));
    }

    public async Task<SignInResult> CustomPreSignInCheck(NuagesApplicationUser<string> user)
    {
        var res = await PreSignInCheck(user);

        return res;
    }

    public override async Task SignOutAsync()
    {
        await Context.SignOutAsync(NuagesIdentityConstants.EmailNotVerifiedScheme);
        await Context.SignOutAsync(NuagesIdentityConstants.ResetPasswordScheme);
        await Context.SignOutAsync(NuagesIdentityConstants.PasswordExpiredScheme);
        
        await base.SignOutAsync();
    }
    
    public override async Task<SignInResult> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberClient)
    {
        var res = await base.TwoFactorSignInAsync(provider, code, isPersistent, rememberClient);
        if (res.Succeeded)
        {
            switch (provider)
            {
                case "FIDO2":
                {
                    var result = await Context.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
                    if (result.Principal != null)
                    {
                        var id = result.Principal.FindFirstValue(ClaimTypes.Name);
                        var user = await UserManager.FindByIdAsync(id);
                        if (user != null)
                        {
                            user.PreferredMfaMethod = "SecurityKeys";
                            var updateRes = await UserManager.UpdateAsync(user);
                            if (!updateRes.Succeeded)
                            {
                                Logger.LogError(updateRes.Errors.First().Description);
                            }
                        }
                    }

                    break;
                }
            }
        }

        return res;
    }
}

public static class NuagesIdentityConstants
{
    public const string EmailNotVerifiedScheme = "EmailNotVerifiedScheme";
    public const string ResetPasswordScheme = "ResetPasswordScheme";
    public const string PasswordExpiredScheme = "PasswordExpiredScheme";
}