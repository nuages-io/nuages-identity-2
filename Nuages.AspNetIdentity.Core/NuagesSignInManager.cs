using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Nuages.AspNetIdentity.Core;

// ReSharper disable once ClassNeverInstantiated.Global
public class NuagesSignInManager : SignInManager<NuagesApplicationUser>
{
    private readonly IUserConfirmation<NuagesApplicationUser> _confirmation;
    private readonly NuagesIdentityOptions _nuagesIdentityOptions;

    // ReSharper disable once MemberCanBeProtected.Global
    public NuagesSignInManager(UserManager<NuagesApplicationUser> userManager, 
        IHttpContextAccessor contextAccessor, 
        IUserClaimsPrincipalFactory<NuagesApplicationUser> claimsFactory, 
        // ReSharper disable once ContextualLoggerProblem
        IOptions<IdentityOptions> optionsAccessor, 
        ILogger<SignInManager<NuagesApplicationUser>> logger, 
        IAuthenticationSchemeProvider schemes, 
        IUserConfirmation<NuagesApplicationUser> confirmation, 
        IOptions<NuagesIdentityOptions> nuagesIdentityOptions) : 
        base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _confirmation = confirmation;
        _nuagesIdentityOptions = nuagesIdentityOptions.Value;
    }

    public override async Task<bool> CanSignInAsync(NuagesApplicationUser user)
    {
        if (!await CheckStartEndAsync(user)) 
            return false;

        if (!await CheckEmailAsync(user)) 
            return false;
        
        if (!await CheckPhoneNumberAsync(user)) 
            return false;
        
        return await CheckAccountAsync(user);
    }
    
    public override async Task<SignInResult> CheckPasswordSignInAsync(NuagesApplicationUser user, string password, bool lockoutOnFailure)
    {
        var res = await base.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
        
        if (res.Succeeded)
        {
            if (!await CheckPasswordAsync(user))
                return SignInResult.NotAllowed;
        }
        else
        {
            if (!res.IsLockedOut && !res.IsNotAllowed && !res.RequiresTwoFactor)
            {
                user.LastFailedLoginReason = FailedLoginReason.UserNameOrPasswordInvalid;
                await UserManager.UpdateAsync(user);
            }
        }

        return res;
    }

    public  async Task<bool> CheckAccountAsync(NuagesApplicationUser user)
    {
        if (Options.SignIn.RequireConfirmedAccount && !await _confirmation.IsConfirmedAsync(UserManager, user))
        {
            user.LastFailedLoginReason = FailedLoginReason.AccountNotConfirmed;

            await UserManager.UpdateAsync(user);
            
           
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

    public async Task<bool> CheckPhoneNumberAsync(NuagesApplicationUser user)
    {
        if (Options.SignIn.RequireConfirmedPhoneNumber && !await UserManager.IsPhoneNumberConfirmedAsync(user))
        {
            user.LastFailedLoginReason = FailedLoginReason.PhoneNotConfirmed;

            await UserManager.UpdateAsync(user);
            return false;
        }

        return true;
    }

    private async Task<bool> CheckEmailAsync(NuagesApplicationUser user)
    {
        if (Options.SignIn.RequireConfirmedEmail && !await UserManager.IsEmailConfirmedAsync(user))
        {
            user.LastFailedLoginReason = FailedLoginReason.EmailNotConfirmed;
            
            await UserManager.UpdateAsync(user);

            return false;
        }

        return true;
    }
    
    

    private async Task<bool> CheckPasswordAsync(NuagesApplicationUser user)
    {
        if (user.UserMustChangePassword)
        {
            user.LastFailedLoginReason = FailedLoginReason.PasswordMustBeChanged;

            await UserManager.UpdateAsync(user);

            var newCode = UserManager.GeneratePasswordResetTokenAsync(user).Result;
            newCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(newCode));
            
            await Context.SignInAsync(NuagesIdentityConstants.PasswordExpiredScheme, StoreAuthInfo("PasswordExpired", user.Id, user.Email, newCode));
            
            return false;
        }

        if (_nuagesIdentityOptions.EnableAutoPasswordExpiration)
        {
            if (!user.LastPasswordChangedDate.HasValue)
            {
                throw new NotSupportedException("PasswordWasNeverSet");
            }

            if (user.EnableAutoExpirePassword)
            {
                if (DateTimeOffset.UtcNow >
                    user.LastPasswordChangedDate.Value.AddDays(_nuagesIdentityOptions.AutoExpirePasswordDelayInDays))
                {
                    user.LastFailedLoginReason = FailedLoginReason.PasswordExpired;

                    await UserManager.UpdateAsync(user);

                    var newCode = UserManager.GeneratePasswordResetTokenAsync(user).Result;
                    newCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(newCode));
                    
                    await Context.SignInAsync(NuagesIdentityConstants.PasswordExpiredScheme, StoreAuthInfo("PasswordExpired", user.Id, user.Email, newCode));
                    
                    return false;
                }
            }
        }

        return true;
    }

    public async Task<bool> CheckStartEndAsync(NuagesApplicationUser user)
    {
        if (user.ValidFrom.HasValue)
        {
            if (user.ValidFrom > DateTimeOffset.UtcNow)
            {
                user.LastFailedLoginReason = FailedLoginReason.NotWithinDateRange;

                await UserManager.UpdateAsync(user);

                return false;
            }
        }

        if (user.ValidTo.HasValue)
        {
            if (DateTime.UtcNow > user.ValidTo)
            {
                user.LastFailedLoginReason = FailedLoginReason.NotWithinDateRange;

                await UserManager.UpdateAsync(user);

                return false;
            }
        }

        return true;
    }

    [ExcludeFromCodeCoverage]
    public override async Task SignInWithClaimsAsync(NuagesApplicationUser user, AuthenticationProperties authenticationProperties,
        IEnumerable<Claim> additionalClaims)
    {
        user.LastLogin = DateTime.UtcNow;
        user.LoginCount++;
        user.LockoutEnd = null;
        user.LockoutMessageSent = false;
        user.AccessFailedCount = 0;

        await UserManager.UpdateAsync(user);
        
        await base.SignInWithClaimsAsync(user, authenticationProperties, additionalClaims);
    }

    protected override async Task<SignInResult> LockedOut(NuagesApplicationUser user)
    {
        user.LastFailedLoginReason = FailedLoginReason.LockedOut;

        await UserManager.UpdateAsync(user);
        
        return await base.LockedOut(user);
    }

    public  Task<SignInResult> CustomSignInOrTwoFactorAsync(NuagesApplicationUser user, bool isPersistent, string? loginProvider = null, bool bypassTwoFactor = false)
    {
        return base.SignInOrTwoFactorAsync(user, isPersistent, loginProvider, bypassTwoFactor);
    }

    public async Task SignInEmailNotVerified(NuagesApplicationUser user)
    {
        await Context.SignInAsync(NuagesIdentityConstants.EmailNotVerifiedScheme, StoreAuthInfo("EmailNotConfirmed", user.Id, user.Email ));
    }

    public async Task<SignInResult> CustomPreSignInCheck(NuagesApplicationUser user)
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
}

public static class NuagesIdentityConstants 
{
    public const string EmailNotVerifiedScheme = "EmailNotVerifiedScheme";
    public const string ResetPasswordScheme = "ResetPasswordScheme";
    public const string PasswordExpiredScheme = "PasswordExpiredScheme";
}