using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.Services.Tests;

public class FakeSignInManager : NuagesSignInManager
{
    public FakeSignInManager(NuagesUserManager userManager, IOptions<NuagesIdentityOptions> options, IHttpContextAccessor? contextAccessor = null)
        : base(userManager,
            contextAccessor ?? MockHelpers.MockHttpContextAccessor().Object,
            new Mock<IUserClaimsPrincipalFactory<NuagesApplicationUser>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<NuagesApplicationUser>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<NuagesApplicationUser>>().Object,
            options)
    { }


    public async override Task SignInWithClaimsAsync(NuagesApplicationUser user, AuthenticationProperties authenticationProperties,
        IEnumerable<Claim> additionalClaims)
    {
        
    }

    public override async Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent,
        bool rememberClient)
    {
        if (code == "ok")
            return await Task.FromResult(SignInResult.Success);
        
        return await Task.FromResult(SignInResult.Failed);
    }

    public NuagesApplicationUser CurrentUser { get; set; }
    public override async Task<NuagesApplicationUser> GetTwoFactorAuthenticationUserAsync()
    {
        return await Task.FromResult(CurrentUser);
    }
}