using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Nuages.Identity.Services.AspNetIdentity;

public class NuagesSignInManager : SignInManager<NuagesApplicationUser>
{
    private readonly IUserConfirmation<NuagesApplicationUser> _confirmation;
    private readonly NuagesIdentityOptions _nuagesIdentityOptions;

    public NuagesSignInManager(UserManager<NuagesApplicationUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<NuagesApplicationUser> claimsFactory, 
        // ReSharper disable once ContextualLoggerProblem
        IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<NuagesApplicationUser>> logger, IAuthenticationSchemeProvider schemes, 
        IUserConfirmation<NuagesApplicationUser> confirmation, IOptions<NuagesIdentityOptions> nuagesIdentityOptions) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
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
        
        if (!await CheckAccountAsync(user)) 
            return false;

        user.LastFailedLoginReason = null;
        
        return true;
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

    private async Task<bool> CheckAccountAsync(NuagesApplicationUser user)
    {
        if (Options.SignIn.RequireConfirmedAccount && !await _confirmation.IsConfirmedAsync(UserManager, user))
        {
            user.LastFailedLoginReason = FailedLoginReason.AccountNotConfirmed;

            await UserManager.UpdateAsync(user);
            
           
            return false;
        }

        return true;
    }


    private ClaimsPrincipal StoreConfirmEmailInfo(string userId, string email)
    {
        var identity = new ClaimsIdentity("EmailNotConfirmed");
        identity.AddClaim(new Claim(ClaimTypes.Name, userId));
        identity.AddClaim(new Claim(ClaimTypes.Email, email));
        return new ClaimsPrincipal(identity);
    }

    private async Task<bool> CheckPhoneNumberAsync(NuagesApplicationUser user)
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
            
            await Context.SignInAsync(NuagesIdentityConstants.EmailNotVerifiedScheme, StoreConfirmEmailInfo(user.Id, user.Email));
            
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

            return false;
        }

        if (_nuagesIdentityOptions.SupportsAutoPasswordExpiration)
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

                    return false;
                }
            }
        }

        return true;
    }

    private async Task<bool> CheckStartEndAsync(NuagesApplicationUser user)
    {
        if (_nuagesIdentityOptions.SupportsStartEnd)
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
        }

        return true;
    }

    public override async Task SignInWithClaimsAsync(NuagesApplicationUser user, AuthenticationProperties authenticationProperties,
        IEnumerable<Claim> additionalClaims)
    {
        user.LastLogin = DateTime.UtcNow;
        user.LoginCount++;
        user.LockoutEnd = null;
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
}

public static class NuagesIdentityConstants 
{
    public const string EmailNotVerifiedScheme = "EmailNotVerifiedScheme";
    public const string ResetPasswordScheme = "ResetPasswordScheme";
}