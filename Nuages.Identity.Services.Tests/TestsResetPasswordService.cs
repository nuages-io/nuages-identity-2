using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Password;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsResetPasswordService
{
    [Fact]
    public async Task ShouldResetPasswordWithSuccess()
    {
        const string password = "Password123*#";
        
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "TEST@NUAGES.ORG",
            NormalizedEmail = "TEST@NUAGES.ORG",
            EmailConfirmed = false
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);

        var resetService = new ResetPasswordService(identityStuff.UserManager, new FakeStringLocalizer());

        var code = await identityStuff.UserManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        var res = await resetService.Reset(new ResetPasswordModel
        {
            Email = user.Email,
            Code = code,
            Password = password,
            PasswordConfirm = password
        });
        
        Assert.True(res.Success);
    }
    
    [Fact]
    public async Task ShouldResetPasswordWithError()
    {
        const string password = "Password123*#";
        
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "TEST@NUAGES.ORG",
            NormalizedEmail = "TEST@NUAGES.ORG",
            EmailConfirmed = true
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);

        var resetService = new ResetPasswordService(identityStuff.UserManager, new FakeStringLocalizer());

        var code = await identityStuff.UserManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        var res = await resetService.Reset(new ResetPasswordModel
        {
            Email = "bad_email",
            Code = code,
            Password = password,
            PasswordConfirm = password
        });
        
        Assert.True(res.Success);
        Assert.True(!res.Errors.Any());
    }
    
    [Fact]
    public async Task ShouldResetPasswordWithErrorNotCOmplexEnough()
    {
        const string password = "password";
        
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "TEST@NUAGES.ORG",
            NormalizedEmail = "TEST@NUAGES.ORG",
            EmailConfirmed = true
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);

        var resetService = new ResetPasswordService(identityStuff.UserManager, new FakeStringLocalizer());

        var code = await identityStuff.UserManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        var res = await resetService.Reset(new ResetPasswordModel
        {
            Email = user.Email,
            Code = code,
            Password = password,
            PasswordConfirm = password
        });
        
        Assert.False(res.Success);
        Assert.Equal("identity.PasswordRequiresNonAlphanumeric", res.Errors[0]);
        Assert.Equal("identity.PasswordRequiresDigit", res.Errors[1]);
        Assert.Equal("identity.PasswordRequiresUpper", res.Errors[2]);
    }
    
    [Fact]
    public async Task ShouldResetPasswordWithErrorPasswordDoesnotMatch()
    {
        const string password = "Password123*#";
        
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "TEST@NUAGES.ORG",
            NormalizedEmail = "TEST@NUAGES.ORG",
            EmailConfirmed = false
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);

        var resetService = new ResetPasswordService(identityStuff.UserManager, new FakeStringLocalizer());

        var code = await identityStuff.UserManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        var res = await resetService.Reset(new ResetPasswordModel
        {
            Email = user.Email,
            Code = code,
            Password = password,
            PasswordConfirm = "bad_password"
        });
        
        Assert.False(res.Success);
        Assert.Equal("resetPassword:passwordConfirmDoesNotMatch", res.Errors.Single());
    }
}