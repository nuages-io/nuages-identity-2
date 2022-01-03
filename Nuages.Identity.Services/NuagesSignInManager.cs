using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Nuages.Identity.Services;

public class NuagesSignInManager : SignInManager<NuagesApplicationUser>
{
    private readonly IUserConfirmation<NuagesApplicationUser> _confirmation;

    public NuagesSignInManager(UserManager<NuagesApplicationUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<NuagesApplicationUser> claimsFactory, 
        // ReSharper disable once ContextualLoggerProblem
        IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<NuagesApplicationUser>> logger, IAuthenticationSchemeProvider schemes, 
        IUserConfirmation<NuagesApplicationUser> confirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        _confirmation = confirmation;
    }

    public override async Task<bool> CanSignInAsync(NuagesApplicationUser user)
    {
        //TODO verifier SupportsStartEnd avec data debut et de fin
        
        if (Options.SignIn.RequireConfirmedEmail && !(await UserManager.IsEmailConfirmedAsync(user)))
        {
            //Logger.LogWarning(EventIds.UserCannotSignInWithoutConfirmedEmail, "User cannot sign in without a confirmed email.");
            return false;
        }
        if (Options.SignIn.RequireConfirmedPhoneNumber && !(await UserManager.IsPhoneNumberConfirmedAsync(user)))
        {
            //Logger.LogWarning(EventIds.UserCannotSignInWithoutConfirmedPhoneNumber, "User cannot sign in without a confirmed phone number.");
            return false;
        }
        
        if (Options.SignIn.RequireConfirmedAccount && !(await _confirmation.IsConfirmedAsync(UserManager, user)))
        {
            //Logger.LogWarning(EventIds.UserCannotSignInWithoutConfirmedAccount, "User cannot sign in without a confirmed account.");
            return false;
        }
        
        return true;
    }
    
    
}