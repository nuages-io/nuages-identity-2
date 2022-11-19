using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Moq;
using Nuages.Identity.Services.Password;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsResetPasswordService
{
    [Fact]
    public async Task ShouldResetPasswordWithSuccess()
    {
        const string password = MockHelpers.StrongPassword;

        var user = MockHelpers.CreateDefaultUser();
        Assert.NotNull(user.Email);
        user.EmailConfirmed = false;

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var resetService = new ResetPasswordService(identityStuff.UserManager, new FakeStringLocalizer(), new Mock<ILogger<ResetPasswordService>>().Object, new Mock<IIdentityEventBus>().Object);

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
        const string password = MockHelpers.StrongPassword;

        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var resetService = new ResetPasswordService(identityStuff.UserManager, new FakeStringLocalizer(), new Mock<ILogger<ResetPasswordService>>().Object, new Mock<IIdentityEventBus>().Object);

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

        var user = MockHelpers.CreateDefaultUser();
        Assert.NotNull(user.Email);
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var resetService = new ResetPasswordService(identityStuff.UserManager, new FakeStringLocalizer(), new Mock<ILogger<ResetPasswordService>>().Object, new Mock<IIdentityEventBus>().Object);

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
        const string password = MockHelpers.StrongPassword;

        var user = MockHelpers.CreateDefaultUser();
        Assert.NotNull(user.Email);
        user.EmailConfirmed = false;

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var resetService = new ResetPasswordService(identityStuff.UserManager, new FakeStringLocalizer(), new Mock<ILogger<ResetPasswordService>>().Object, new Mock<IIdentityEventBus>().Object);

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