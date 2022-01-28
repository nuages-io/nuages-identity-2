using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Manage;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsChangePasswordService
{
    [Fact]
    public async Task ShouldChangePasswordWithSuccess()
    {
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@nuages.org",
            NormalizedEmail = "TEST@NUAGES.ORG",
            PasswordHash = "password_hash"
        };

        var currentPssword = "Current789*$";
        var newPassword = "NewPassword789*$";

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, currentPssword);
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        var res = await changePasswordService.ChangePasswordAsync(user.Id, currentPssword, newPassword, newPassword);
        
        Assert.True(res.Success);
        Assert.True(sendCalled);
    }
    
    [Fact]
    public async Task ShouldAddPasswordWithSuccess()
    {
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@nuages.org",
            NormalizedEmail = "TEST@NUAGES.ORG",
        };

        var newPassword = "NewPassword789*$";
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        var res = await changePasswordService.AddPasswordAsync(user.Id, newPassword, newPassword);
        
        Assert.True(res.Success);
        Assert.True(sendCalled);
    }
    
    [Fact]
    public async Task ShouldChangePasswordWithErrorCurrentDoesNotMatch()
    {
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@nuages.org",
            NormalizedEmail = "TEST@NUAGES.ORG",
            PasswordHash = "password_hash"
        };

        var currentPssword = "Current789*$";
        var newPassword = "NewPassword789*$";

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, currentPssword);
        
        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(), new Mock<IMessageService>().Object);

        var res = await changePasswordService.ChangePasswordAsync(user.Id, "bad_current", newPassword, newPassword);
        
        Assert.False(res.Success);
        Assert.Equal("identity.PasswordMismatch", res.Errors.First());
    }
    
    [Fact]
    public async Task ShouldChangePasswordWithErrorConfirmationDoesNotMatch()
    {
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@nuages.org",
            NormalizedEmail = "TEST@NUAGES.ORG",
            PasswordHash = "password_hash"
        };

        var currentPssword = "Current789*$";
        var newPassword = "NewPassword789*$";
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, currentPssword);

        var messageService = new Mock<IMessageService>();

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        var res = await changePasswordService.ChangePasswordAsync(user.Id, currentPssword, newPassword, "bad_confirm");
        
        Assert.False(res.Success);
        Assert.Equal("resetPassword.passwordConfirmDoesNotMatch", res.Errors.First());
    }
    
    [Fact]
    public async Task ShouldChangePasswordWitExceptionUserNotFound()
    {
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@nuages.org",
            NormalizedEmail = "TEST@NUAGES.ORG",
            PasswordHash = "password_hash"
        };

        var currentPssword = "Current789*$";
        var newPassword = "NewPassword789*$";
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, currentPssword);

        var messageService = new Mock<IMessageService>();

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await changePasswordService.ChangePasswordAsync("bad_id", currentPssword, newPassword, newPassword);
        });
    }
}