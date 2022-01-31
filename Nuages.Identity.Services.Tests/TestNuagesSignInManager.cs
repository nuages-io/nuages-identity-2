using System.Threading.Tasks;
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

    }

    [Fact]
    public async Task ShouldCCanSignInWithSuccess()
    {

        var user = MockHelpers.CreateDefaultUser();
        user.PhoneNumber = "9999999999";
        user.PhoneNumberConfirmed = true;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await identityStuff.SignInManager.CanSignInAsync(user);
    
        Assert.True(res);

    }

}