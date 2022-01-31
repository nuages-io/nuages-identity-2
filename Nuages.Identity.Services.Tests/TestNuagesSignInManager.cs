using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.Services.Login;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestNuagesSignInManager
{
    [Fact]
    public async Task ShouldCheckAccountWithSuccess()
    {

        var user = MockHelpers.CreateDefaultUser();
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        
        var res = await identityStuff.SignInManager.CheckAccountAsync(user);
        
        Assert.True(res);

    }
    
    [Fact]
    public async Task ShouldCheckAccountWithErrorNotConfirmed()
    {

        var user = MockHelpers.CreateDefaultUser();
        user.EmailConfirmed = false;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        
        var res = await identityStuff.SignInManager.CheckAccountAsync(user);
        
        Assert.False(res);

        Assert.False(await identityStuff.SignInManager.CanSignInAsync(user));
    }
    
    [Fact]
    public async Task ShouldCheckPasswordSignInAsyncWIthErrorMustChangePassword()
    {
        var password = "Nuages789*$";
        
        var user = MockHelpers.CreateDefaultUser();
        user.UserMustChangePassword = true;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);

        
        var res = await identityStuff.SignInManager.CheckPasswordSignInAsync(user, password, false);
        
        Assert.False(res.Succeeded);
        Assert.Equal(FailedLoginReason.PasswordMustBeChanged, user.LastFailedLoginReason);
    }

    [Fact]
    public async Task ShouldSignOutWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        await identityStuff.SignInManager.SignOutAsync();
    }


    [Fact]
    public async Task ShouldCheckhoneNUmberWithSuccess()
    {

        var user = MockHelpers.CreateDefaultUser();
        user.PhoneNumber = "9999999999";
        user.PhoneNumberConfirmed = true;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await identityStuff.SignInManager.CheckPhoneNumberAsync(user);
    
        Assert.True(res);

    }
    
    [Fact]
    public async Task ShouldCheckhoneNUmberWithErrorNotCOnfirm()
    {

        var user = MockHelpers.CreateDefaultUser();
        user.PhoneNumber = "9999999999";
        user.PhoneNumberConfirmed = false;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await identityStuff.SignInManager.CheckPhoneNumberAsync(user);
    
        Assert.False(res);
        Assert.False(await identityStuff.SignInManager.CanSignInAsync(user));
    }

    [Fact]
    public async Task ShouldCanSignInWithSuccess()
    {

        var user = MockHelpers.CreateDefaultUser();
        user.PhoneNumber = "9999999999";
        user.PhoneNumberConfirmed = true;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await identityStuff.SignInManager.CanSignInAsync(user);
    
        Assert.True(res);

    }
    
    [Fact]
    public async Task ShouldCheckStartEndWithSuccess()
    {

        var user = MockHelpers.CreateDefaultUser();
        user.ValidFrom = DateTime.Now.AddDays(-1);
        user.ValidTo = DateTime.Now.AddDays(1);
        
        var identityStuff = MockHelpers.MockIdentityStuff(user, new NuagesIdentityOptions
        {
            SupportsStartEnd = true
        });
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await identityStuff.SignInManager.CheckStartEndAsync(user);
    
        Assert.True(res);

    }
    
    [Fact]
    public async Task ShouldCheckStartEndWithErrorValidAfter()
    {

        var user = MockHelpers.CreateDefaultUser();
        user.ValidFrom = DateTime.Now.AddDays(2);
        user.ValidTo = DateTime.Now.AddDays(3);
        
        var identityStuff = MockHelpers.MockIdentityStuff(user, new NuagesIdentityOptions
        {
            SupportsStartEnd = true
        });
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await identityStuff.SignInManager.CheckStartEndAsync(user);
    
        Assert.False(res);

        Assert.False(await identityStuff.SignInManager.CanSignInAsync(user));
        
    }
    
    [Fact]
    public async Task ShouldCheckStartEndWithErrorValidBefore()
    {

        var user = MockHelpers.CreateDefaultUser();
        user.ValidFrom = DateTime.Now.AddDays(-3);
        user.ValidTo = DateTime.Now.AddDays(-2);
        
        var identityStuff = MockHelpers.MockIdentityStuff(user, new NuagesIdentityOptions
        {
            SupportsStartEnd = true
        });
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await identityStuff.SignInManager.CheckStartEndAsync(user);
    
        Assert.False(res);

    }
    
    [Fact]
    public async Task ShouldSignInWithClaimWithSuccess()
    {

        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user, new NuagesIdentityOptions
        {
            SupportsStartEnd = true
        });
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        await identityStuff.SignInManager.SignInWithClaimsAsync(user, new AuthenticationProperties(), new List<Claim>());
    
    }

    [Fact]
    public async Task ShouldCheckPasswordSignInAsyncWIthErrorPasswordExpired()
    {
        var password = "Nuages789*$";
        
        var user = MockHelpers.CreateDefaultUser();
        user.EnableAutoExpirePassword = true;
        user.LastPasswordChangedDate = DateTime.Now.AddYears(-2);
        
        var identityStuff = MockHelpers.MockIdentityStuff(user, new NuagesIdentityOptions
        {
            EnableAutoPasswordExpiration = true
        });
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);

        
        var res = await identityStuff.SignInManager.CheckPasswordSignInAsync(user, password, false);
        
        Assert.False(res.Succeeded);
        Assert.Equal(FailedLoginReason.PasswordExpired, user.LastFailedLoginReason);
    }
    
    [Fact]
    public async Task ShouldCheckPasswordSignInAsyncWIthExceptionPasswordDateNotProvided()
    {
        var password = "Nuages789*$";
        
        var user = MockHelpers.CreateDefaultUser();
        user.EnableAutoExpirePassword = true;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user, new NuagesIdentityOptions
        {
            EnableAutoPasswordExpiration = true
        });
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);
        
        var ex = await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            await identityStuff.SignInManager.CheckPasswordSignInAsync(user, password, false);
        });
        
        Assert.Equal("PasswordWasNeverSet", ex.Message);
    }
    
    [Fact]
    public async Task ShouldCheckPasswordSignInAsyncWIthSuccessPasswordExpiredButNotEnabled()
    {
        var password = "Nuages789*$";
        
        var user = MockHelpers.CreateDefaultUser();
        user.EnableAutoExpirePassword = false;
        user.LastPasswordChangedDate = DateTime.Now.AddYears(-2);
        
        var identityStuff = MockHelpers.MockIdentityStuff(user, new NuagesIdentityOptions
        {
            EnableAutoPasswordExpiration = true
        });
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);

        
        var res = await identityStuff.SignInManager.CheckPasswordSignInAsync(user, password, false);
        
        Assert.True(res.Succeeded);
    }
    
    [Fact]
    public async Task ShouldCheckPasswordSignInAsyncWIthSuccessPasswordNotExpired()
    {
        var password = "Nuages789*$";
        
        var user = MockHelpers.CreateDefaultUser();
        user.EnableAutoExpirePassword = true;
        user.LastPasswordChangedDate = DateTime.Now.AddHours(-1);
        
        var identityStuff = MockHelpers.MockIdentityStuff(user, new NuagesIdentityOptions
        {
            EnableAutoPasswordExpiration = true
        });
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);

        
        var res = await identityStuff.SignInManager.CheckPasswordSignInAsync(user, password, false);
        
        Assert.True(res.Succeeded);
    }
}