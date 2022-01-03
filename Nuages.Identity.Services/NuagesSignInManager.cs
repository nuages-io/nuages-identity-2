using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Nuages.Identity.Services;

public class NuagesSignInManager : SignInManager<NuagesApplicationUser>
{
    public NuagesSignInManager(UserManager<NuagesApplicationUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<NuagesApplicationUser> claimsFactory, 
        // ReSharper disable once ContextualLoggerProblem
        IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<NuagesApplicationUser>> logger, IAuthenticationSchemeProvider schemes, 
        IUserConfirmation<NuagesApplicationUser> confirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }
    
}