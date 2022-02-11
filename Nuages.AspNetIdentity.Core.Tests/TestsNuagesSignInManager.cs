using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Xunit;

namespace Nuages.AspNetIdentity.Core.Tests;

public class TestNuagesSignInManager
{
    private readonly NuagesApplicationUser<string> _defaultUser;
    private readonly MockHelpers.MockIdentity _identityStuff;
    public TestNuagesSignInManager()
    {
        _defaultUser = new NuagesApplicationUser<string>
        {
            Email = "test@example.com",
            EmailConfirmed = true,
            UserName = "test"
        };
        
        _identityStuff = MockHelpers.MockIdentityStuff();
    }
    
    [Fact]
    public async Task ShouldCheckAccountWithSuccess()
    {
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        
        var res = await _identityStuff.SignInManager.CheckAccountAsync(_defaultUser);
        
        Assert.True(res);
    }
    
    [Fact]
    public async Task ShouldCheckAccountWithErrorNotConfirmed()
    {
        _defaultUser.EmailConfirmed = false;
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        
        var res = await _identityStuff.SignInManager.CheckAccountAsync(_defaultUser);
        
        Assert.False(res);
    }
    
    [Fact]
    public async Task ShouldNotBeAbleToSignIn()
    {
       
        _defaultUser.EmailConfirmed = false;
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        
        Assert.False(await _identityStuff.SignInManager.CanSignInAsync(_defaultUser));
    }
    
    [Fact]
    public async Task ShouldBeAbleToSignInWithSuccess()
    {
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        
        Assert.True(await _identityStuff.SignInManager.CanSignInAsync(_defaultUser));
    }
    
    [Fact]
    public async Task ShouldCheckPasswordSignInAsyncWIthErrorMustChangePassword()
    {
        const string password = MockHelpers.StrongPassword;

        _defaultUser.UserMustChangePassword = true;
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        _defaultUser.PasswordHash = _identityStuff.UserManager.PasswordHasher.HashPassword(_defaultUser, password);
    
        
        var res = await _identityStuff.SignInManager.CheckPasswordSignInAsync(_defaultUser, password, false);
        
        Assert.False(res.Succeeded);
        Assert.Equal(FailedLoginReason.PasswordMustBeChanged, _defaultUser.LastFailedLoginReason);
    }
    
    [Fact]
    public async Task ShouldSignOutWithSuccess()
    {
        await _identityStuff.SignInManager.SignOutAsync();
    }
    
    
    [Fact]
    public async Task ShouldCheckhoneNUmberWithSuccess()
    {
        _defaultUser.PhoneNumber = MockHelpers.PhoneNumber;
        _defaultUser.PhoneNumberConfirmed = true;
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await _identityStuff.SignInManager.CheckPhoneNumberAsync(_defaultUser);
    
        Assert.True(res);
    
    }
    
    [Fact]
    public async Task ShouldCheckPhoneNumberWithErrorNotConfirm()
    {
    
        _defaultUser.PhoneNumber = MockHelpers.PhoneNumber;
        _defaultUser.PhoneNumberConfirmed = false;
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await _identityStuff.SignInManager.CheckPhoneNumberAsync(_defaultUser);
    
        Assert.False(res);
        
        Assert.False(await _identityStuff.SignInManager.CanSignInAsync(_defaultUser));
    }
    
    [Fact]
    public async Task ShouldCanSignInWithSuccess()
    {
    
        _defaultUser.PhoneNumber = MockHelpers.PhoneNumber;
        _defaultUser.PhoneNumberConfirmed = true;
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await _identityStuff.SignInManager.CanSignInAsync(_defaultUser);
    
        Assert.True(res);
    
    }
    
    [Fact]
    public async Task ShouldCheckStartEndWithSuccess()
    {
        _defaultUser.ValidFrom = DateTime.Now.AddDays(-1);
        _defaultUser.ValidTo = DateTime.Now.AddDays(1);
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await _identityStuff.SignInManager.CheckStartEndAsync(_defaultUser);
    
        Assert.True(res);
    
    }
    
    [Fact]
    public async Task ShouldCheckStartEndWithErrorValidAfter()
    {
    
        _defaultUser.ValidFrom = DateTime.Now.AddDays(2);
        _defaultUser.ValidTo = DateTime.Now.AddDays(3);
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await _identityStuff.SignInManager.CheckStartEndAsync(_defaultUser);
    
        Assert.False(res);
    
        Assert.False(await _identityStuff.SignInManager.CanSignInAsync(_defaultUser));
        
    }
    
    [Fact]
    public async Task ShouldCheckStartEndWithErrorValidBefore()
    {
        _defaultUser.ValidFrom = DateTime.Now.AddDays(-3);
        _defaultUser.ValidTo = DateTime.Now.AddDays(-2);
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);

        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        var res = await _identityStuff.SignInManager.CheckStartEndAsync(_defaultUser);
    
        Assert.False(res);
    
    }
    
    [Fact]
    public async Task ShouldSignInWithClaimWithSuccess()
    {
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedPhoneNumber = true;
    
        await _identityStuff.SignInManager.SignInWithClaimsAsync(_defaultUser, new AuthenticationProperties(), new List<Claim>());
    
    }
    
    [Fact]
    public async Task ShouldCheckPasswordSignInAsyncWIthErrorPasswordExpired()
    {
        const string password = MockHelpers.StrongPassword;
        
        _defaultUser.EnableAutoExpirePassword = true;
        _defaultUser.PasswordHash = _identityStuff.UserManager.PasswordHasher.HashPassword(_defaultUser, password);
       
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);

        _identityStuff.NuagesOptions.EnableAutoPasswordExpiration = true;
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        
        _defaultUser.LastPasswordChangedDate = DateTime.Now.AddYears(-2);
        
        var res = await _identityStuff.SignInManager.CheckPasswordSignInAsync(_defaultUser, password, false);
        
        Assert.False(res.Succeeded);
        Assert.Equal(FailedLoginReason.PasswordExpired, _defaultUser.LastFailedLoginReason);
    }
    
    [Fact]
    public async Task ShouldCheckPasswordSignInAsyncWIthExceptionPasswordDateNotProvided()
    {
        const string password = MockHelpers.StrongPassword;
        
        _defaultUser.EnableAutoExpirePassword = true;
        _defaultUser.PasswordHash = _identityStuff.UserManager.PasswordHasher.HashPassword(_defaultUser, password);
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
        
        _identityStuff.NuagesOptions.EnableAutoPasswordExpiration = true;
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;

        _defaultUser.LastPasswordChangedDate = null;
        
        
        var ex = await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            await _identityStuff.SignInManager.CheckPasswordSignInAsync(_defaultUser, password, false);
        });
        
        Assert.Equal("PasswordWasNeverSet", ex.Message);
    }
    
    [Fact]
    public async Task ShouldCheckPasswordSignInAsyncWIthSuccessPasswordExpiredButNotEnabled()
    {
        const string password = MockHelpers.StrongPassword;
        
        _defaultUser.EnableAutoExpirePassword = false;
        _defaultUser.LastPasswordChangedDate = DateTime.Now.AddYears(-2);
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);

        _identityStuff.NuagesOptions.EnableAutoPasswordExpiration = true;
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        _defaultUser.PasswordHash = _identityStuff.UserManager.PasswordHasher.HashPassword(_defaultUser, password);
    
        
        var res = await _identityStuff.SignInManager.CheckPasswordSignInAsync(_defaultUser, password, false);
        
        Assert.True(res.Succeeded);
    }
    
    [Fact]
    public async Task ShouldCheckPasswordSignInAsyncWIthSuccessPasswordNotExpired()
    {
        const string password = MockHelpers.StrongPassword;
        
        _defaultUser.EnableAutoExpirePassword = true;
        _defaultUser.LastPasswordChangedDate = DateTime.Now.AddHours(-1);
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);

        _identityStuff.NuagesOptions.EnableAutoPasswordExpiration = true;
        
        _identityStuff.SignInManager.Options.SignIn.RequireConfirmedAccount = true;
        _defaultUser.PasswordHash = _identityStuff.UserManager.PasswordHasher.HashPassword(_defaultUser, password);
    
        
        var res = await _identityStuff.SignInManager.CheckPasswordSignInAsync(_defaultUser, password, false);
        
        Assert.True(res.Succeeded);
    }
}