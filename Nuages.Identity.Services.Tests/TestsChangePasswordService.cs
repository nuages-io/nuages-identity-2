using Moq;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Identity.Services.Manage;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsChangePasswordService
{
    [Fact]
    public async Task ShouldChangePasswordWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        const string currentPassword = MockHelpers.StrongPassword;
        const string newPassword = MockHelpers.StrongPassword + "New";

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, currentPassword);

        var sendCalled = false;

        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        var res = await changePasswordService.ChangePasswordAsync(user.Id, currentPassword, newPassword, newPassword);

        Assert.True(res.Success);
        Assert.True(sendCalled);
    }

    [Fact]
    public async Task ShouldAddPasswordWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        const string newPassword = MockHelpers.StrongPassword + "New";

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
        var user = MockHelpers.CreateDefaultUser();

        const string currentPassword = MockHelpers.StrongPassword;
        const string newPassword = MockHelpers.StrongPassword + "New";

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, currentPassword);

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(),
                new Mock<IMessageService>().Object);

        var res = await changePasswordService.ChangePasswordAsync(user.Id, "bad_current", newPassword, newPassword);

        Assert.False(res.Success);
        Assert.Equal("identity.PasswordMismatch", res.Errors.Single());
    }

    [Fact]
    public async Task ShouldChangePasswordWithErrorConfirmationDoesNotMatch()
    {
        var user = MockHelpers.CreateDefaultUser();

        const string currentPassword = MockHelpers.StrongPassword;
        const string newPassword = MockHelpers.StrongPassword + "New";

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, currentPassword);

        var messageService = new Mock<IMessageService>();

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        var res = await changePasswordService.ChangePasswordAsync(user.Id, currentPassword, newPassword, "bad_confirm");

        Assert.False(res.Success);
        Assert.Equal("resetPassword.passwordConfirmDoesNotMatch", res.Errors.Single());
    }

    [Fact]
    public async Task ShouldChangePasswordWitExceptionUserNotFound()
    {
        var user = MockHelpers.CreateDefaultUser();

        const string currentPassword = MockHelpers.StrongPassword;
        const string newPassword = MockHelpers.StrongPassword + "New";

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, currentPassword);

        var messageService = new Mock<IMessageService>();

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await changePasswordService.ChangePasswordAsync(MockHelpers.BadId, currentPassword, newPassword,
                newPassword);
        });
    }

    [Fact]
    public async Task ShouldAdminChangePasswordWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        const string currentPassword = MockHelpers.StrongPassword;
        const string newPassword = MockHelpers.StrongPassword + "New";

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, currentPassword);

        var sendCalled = false;

        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        var code = await identityStuff.UserManager.GeneratePasswordResetTokenAsync(user);

        var res = await changePasswordService.AdminChangePasswordAsync(user.Id,
            newPassword, newPassword, false, true, code);

        Assert.True(res.Success);
        Assert.True(sendCalled);
    }

    [Fact]
    public async Task ShouldAdminAddPasswordWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        const string newPassword = MockHelpers.StrongPassword + "New";

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var sendCalled = false;

        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        var code = await identityStuff.UserManager.GeneratePasswordResetTokenAsync(user);

        var res = await changePasswordService.AdminChangePasswordAsync(user.Id,
            newPassword, newPassword, false, true, code);

        Assert.True(res.Success);
        Assert.True(sendCalled);
    }


    [Fact]
    public async Task ShouldAdminChangePasswordWithoutTokenWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        const string currentPassword = MockHelpers.StrongPassword;
        const string newPassword = MockHelpers.StrongPassword + "New";

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, currentPassword);

        var sendCalled = false;

        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        var res = await changePasswordService.AdminChangePasswordAsync(user.Id,
            newPassword, newPassword, false, true, null);

        Assert.True(res.Success);
        Assert.True(sendCalled);
    }

    [Fact]
    public async Task ShouldAdminChangePasswordWithFailurePasswordDoesNotMatch()
    {
        var user = MockHelpers.CreateDefaultUser();

        const string currentPassword = MockHelpers.StrongPassword;
        const string newPassword = MockHelpers.StrongPassword + "New";

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, currentPassword);

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(),
                new Mock<IMessageService>().Object);

        var res = await changePasswordService.AdminChangePasswordAsync(user.Id,
            newPassword, "bad_confirm", false, true, null);

        Assert.False(res.Success);
        Assert.Equal("resetPassword.passwordConfirmDoesNotMatch", res.Errors.Single());
        //Assert.True(sendCalled);
    }

    [Fact]
    public async Task ShouldAdminChangePasswordWithFailureThrowException()
    {
        var user = MockHelpers.CreateDefaultUser();

        const string currentPassword = MockHelpers.StrongPassword;
        const string newPassword = MockHelpers.StrongPassword + "New";

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, currentPassword);

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(),
                new Mock<IMessageService>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await changePasswordService.AdminChangePasswordAsync(MockHelpers.BadId,
                newPassword, newPassword, false, true, null);
        });
    }

    [Fact]
    public async Task ShouldAdminChangePasswordWithFailurePasswordNotStrongEnough()
    {
        var user = MockHelpers.CreateDefaultUser();

        const string newPassword = "wpassword";

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var changePasswordService =
            new ChangePasswordService(identityStuff.UserManager, new FakeStringLocalizer(),
                new Mock<IMessageService>().Object);

        var res = await changePasswordService.AdminChangePasswordAsync(user.Id,
            newPassword, newPassword, false, true, null);

        Assert.False(res.Success);
        Assert.Equal("identity.PasswordRequiresNonAlphanumeric", res.Errors[0]);
        Assert.Equal("identity.PasswordRequiresDigit", res.Errors[1]);
        Assert.Equal("identity.PasswordRequiresUpper", res.Errors[2]);
    }
}