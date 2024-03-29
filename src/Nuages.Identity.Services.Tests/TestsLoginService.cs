using Microsoft.Extensions.Logging;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Identity.Services.Login;
using Xunit;
using NotFoundException = Nuages.Web.Exceptions.NotFoundException;

namespace Nuages.Identity.Services.Tests;

public class TestsLoginService
{
    [Fact]
    public async Task ShouldLoginWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        Assert.NotNull(user.Email);
        
        const string password = MockHelpers.StrongPassword;

        var identityStuff = MockHelpers.MockIdentityStuff(user, new NuagesIdentityOptions
        {
            SupportsLoginWithEmail = true,
            AutoExpirePasswordDelayInDays = 90,
            AutoConfirmExternalLogin = true,
            Name = "TestName"
        });

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);


        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            messageService.Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.LoginAsync(new LoginModel
        {
            UserNameOrEmail = user.Email,
            Password = password,
            RememberMe = false
        });

        Assert.True(res.Success);
        Assert.NotNull(user.LastLogin);
    }

    [Fact]
    public async Task ShouldLoginWithFailureWrongPassword()
    {
        const string password = MockHelpers.StrongPassword;

        var user = MockHelpers.CreateDefaultUser();

        Assert.NotNull(user.Email);
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            new Mock<IMessageService>().Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.LoginAsync(new LoginModel
        {
            UserNameOrEmail = user.Email,
            Password = "bad_password",
            RememberMe = false
        });

        Assert.False(res.Success);
        Assert.Equal(FailedLoginReason.UserNameOrPasswordInvalid, res.Reason);
        Assert.NotStrictEqual("errorMessage:userNameOrPasswordInvalid", res.Message);
    }

    [Fact]
    public async Task ShouldLoginWithFailureWrongUsername()
    {
        const string password = MockHelpers.StrongPassword;

        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);

        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            messageService.Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.LoginAsync(new LoginModel
        {
            UserNameOrEmail = "bad_username",
            Password = password,
            RememberMe = false
        });

        Assert.False(res.Success);
        Assert.Equal(FailedLoginReason.UserNameOrPasswordInvalid, res.Reason);
    }


    [Fact]
    public async Task ShouldLoginWithFailureLockedOut()
    {
        const string password = MockHelpers.StrongPassword;

        var user = MockHelpers.CreateDefaultUser();
        
        Assert.NotNull(user.Email);
        
        user.AccessFailedCount = 4;
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.Now.AddDays(1);

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);

        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            messageService.Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.LoginAsync(new LoginModel
        {
            UserNameOrEmail = user.Email,
            Password = password,
            RememberMe = false
        });

        Assert.False(res.Success);
        Assert.Equal(FailedLoginReason.LockedOut, res.Reason);
    }

    [Fact]
    public async Task ShouldLoginWithFailureEmailNotConfirmed()
    {
        const string password = MockHelpers.StrongPassword;

        var user = MockHelpers.CreateDefaultUser();
        Assert.NotNull(user.Email);
        
        user.EmailConfirmed = false;

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        identityStuff.UserManager.Options.SignIn.RequireConfirmedEmail = true;

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);

        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            messageService.Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.LoginAsync(new LoginModel
        {
            UserNameOrEmail = user.Email,
            Password = password,
            RememberMe = false
        });

        Assert.False(res.Success);
        Assert.Equal(FailedLoginReason.EmailNotConfirmed, res.Reason);
    }

    [Fact]
    public async Task ShouldLoginWithFailureAccountNotConfirmed()
    {
        const string password = MockHelpers.StrongPassword;

        var user = MockHelpers.CreateDefaultUser();
        
        Assert.NotNull(user.Email);
        
        user.EmailConfirmed = false;

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        identityStuff.UserManager.Options.SignIn.RequireConfirmedEmail = false;
        identityStuff.UserManager.Options.SignIn.RequireConfirmedAccount = true;

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);

        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            messageService.Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.LoginAsync(new LoginModel
        {
            UserNameOrEmail = user.Email,
            Password = password,
            RememberMe = false
        });

        Assert.False(res.Success);
        Assert.Equal(FailedLoginReason.AccountNotConfirmed, res.Reason);
    }

    [Fact]
    public async Task ShouldLoginWithFailureBadPasswordThenLockedOut()
    {
        const string password = MockHelpers.StrongPassword;

        var user = MockHelpers.CreateDefaultUser();
        
        Assert.NotNull(user.Email);
        
        user.AccessFailedCount = 5;
        user.LockoutEnabled = true;

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            new Mock<IMessageService>().Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.LoginAsync(new LoginModel
        {
            UserNameOrEmail = user.Email,
            Password = "password",
            RememberMe = false
        });

        Assert.False(res.Success);
        Assert.Equal(FailedLoginReason.LockedOut, res.Reason);
    }

    [Fact]
    public async Task ShoudLogin2FaWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            new Mock<IMessageService>().Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.Login2FAAsync(new Login2FAModel
        {
            Code = MockHelpers.ValidToken,
            RememberMachine = false,
            RememberMe = false
        });

        Assert.True(res.Success);
    }

    [Fact]
    public async Task ShoudLogin2FaWithError()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            new Mock<IMessageService>().Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.Login2FAAsync(new Login2FAModel
        {
            Code = "bad_code",
            RememberMachine = false,
            RememberMe = false
        });

        Assert.False(res.Success);
        Assert.Equal(FailedLoginReason.FailedMfa, res.Reason);
    }

    [Fact]
    public async Task ShoudLogin2FaWithErrorUserNotFOund()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.SignInManager.CurrentUser = null;

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            new Mock<IMessageService>().Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await loginService.Login2FAAsync(new Login2FAModel
            {
                Code = MockHelpers.ValidToken,
                RememberMachine = false,
                RememberMe = false
            });
        });
    }

    [Fact]
    public async Task ShoudLoginRecoveryCodeWithSuccess()
    {
        const string code = MockHelpers.ValidRecoveryCode;

        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            messageService.Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.LoginRecoveryCodeAsync(new LoginRecoveryCodeModel
        {
            Code = code
        });

        Assert.True(res.Success);
    }

    [Fact]
    public async Task ShoudLoginRecoveryCodeWithFailureBadCode()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            new Mock<IMessageService>().Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.LoginRecoveryCodeAsync(new LoginRecoveryCodeModel
        {
            Code = "654321" //Bad code
        });

        Assert.False(res.Success);
    }


    [Fact]
    public async Task ShoudLoginRecoveryCodeWithFailuerUserDoesNotExists()
    {
        const string code = MockHelpers.ValidRecoveryCode;

        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.SignInManager.CurrentUser = null;

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            new Mock<IMessageService>().Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await loginService.LoginRecoveryCodeAsync(new LoginRecoveryCodeModel
            {
                Code = code
            });
        });
    }

    [Fact]
    public async Task ShoudLoginSmsWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            new Mock<IMessageService>().Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.LoginSMSAsync(new LoginSMSModel
        {
            Code = MockHelpers.ValidRecoveryCode
        });

        Assert.True(res.Success);
    }

    [Fact]
    public async Task ShoudLoginSmsWithErrorBadCode()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            new Mock<IMessageService>().Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.LoginSMSAsync(new LoginSMSModel
        {
            Code = "654321"
        });

        Assert.False(res.Success);
    }


    [Fact]
    public async Task ShoudLoginSmsWithFailureUserNotFound()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.SignInManager.CurrentUser = null;

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            new Mock<IMessageService>().Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await loginService.LoginSMSAsync(new LoginSMSModel
            {
                Code = MockHelpers.ValidRecoveryCode
            });
        });
    }
    
    [Fact]
    public async Task ShouldLoginWithFailurePasswordMustBeChanged()
    {
        const string password = MockHelpers.StrongPassword;

        var user = MockHelpers.CreateDefaultUser();
        
        Assert.NotNull(user.Email);
        
        user.UserMustChangePassword = true;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager,
            new FakeStringLocalizer(),
            new Mock<IMessageService>().Object, new Mock<ILogger<LoginService>>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await loginService.LoginAsync(new LoginModel
        {
            UserNameOrEmail = user.Email,
            Password = password,
            RememberMe = false
        });

        Assert.False(res.Success);
        Assert.Equal(FailedLoginReason.PasswordMustBeChanged, res.Reason);
        Assert.NotStrictEqual("errorMessage:no_access.error", res.Message);
    }
}