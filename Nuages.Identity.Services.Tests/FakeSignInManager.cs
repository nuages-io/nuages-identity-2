using System.Security.Claims;
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
    public FakeSignInManager(NuagesUserManager userManager, IOptions<NuagesIdentityOptions> options, IUserConfirmation<NuagesApplicationUser> confirmation,
        IOptions<IdentityOptions> identityOptions, IHttpContextAccessor? contextAccessor = null, NuagesApplicationUser? user = null)
        : base(userManager,
            contextAccessor ?? MockHelpers.MockHttpContextAccessor().Object,
            new Mock<IUserClaimsPrincipalFactory<NuagesApplicationUser>>().Object,
            identityOptions,
            new Mock<ILogger<SignInManager<NuagesApplicationUser>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
             confirmation,
            options)
    {

        CurrentUser = user;
        
    }


    public override async  Task SignInWithClaimsAsync(NuagesApplicationUser user, AuthenticationProperties authenticationProperties,
        IEnumerable<Claim> additionalClaims)
    {
        user.LastLogin = DateTime.UtcNow;
        user.LoginCount++;
        user.LockoutEnd = null;
        user.LockoutMessageSent = false;
        user.AccessFailedCount = 0;
        
        await Task.FromResult(0);
    }

    public override async Task<SignInResult> TwoFactorAuthenticatorSignInAsync(string code, bool isPersistent,
        bool rememberClient)
    {
        if (code == "ok")
            return await Task.FromResult(SignInResult.Success);
        
        return await Task.FromResult(SignInResult.Failed);
    }

    public NuagesApplicationUser? CurrentUser { get; set; }
    public override async Task<NuagesApplicationUser> GetTwoFactorAuthenticationUserAsync()
    {
        if (CurrentUser == null)
            return (await Task.FromResult((NuagesApplicationUser?)null))!;
        
        return await Task.FromResult(CurrentUser);
    }

    public override async Task<SignInResult> TwoFactorRecoveryCodeSignInAsync(string recoveryCode)
    {
        return await Task.FromResult(recoveryCode == "123456" ? SignInResult.Success : SignInResult.Failed);
    }

    public override async Task<SignInResult> TwoFactorSignInAsync(string provider, string code, bool isPersistent,
        bool rememberClient)
    {
        return await Task.FromResult(code == "123456" ? SignInResult.Success : SignInResult.Failed);
    }

    public override async Task<ExternalLoginInfo> GetExternalLoginInfoAsync(string? expectedXsrf = null)
    {
        var i = new ClaimsIdentity();
        if (CurrentUser == null)
            return null!;
        
        if (!string.IsNullOrEmpty(CurrentUser.Email))
            i.AddClaim(new Claim(ClaimTypes.Email, CurrentUser.Email));
        
        var p = new ClaimsPrincipal(i);

        var providerName = CurrentUser.Email == "INVALID@NUAGES.ORG" ? "invalid" : "loginProvider";
        
        var info = new ExternalLoginInfo(p, providerName, "providerKey", "displayName");
        
        return await Task.FromResult(info);
    }
}