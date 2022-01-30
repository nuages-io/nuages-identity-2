using System;
using System.Threading.Tasks;
using Moq;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Login;
using Nuages.Web.Exceptions;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Nuages.Identity.Services.Tests;

public class TestsLoginService
{
    [Fact]
    public async Task ShouldLoginWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        const string password = "Nuages123*$";
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);
        
        
        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            messageService.Object);

        var res = await loginService.LoginAsync(new LoginModel
        {
            UserNameOrEmail = user.Email,
            Password = password,
            RememberMe = false
        });
        
        Assert.True(res.Success);
    }
    
    [Fact]
    public async Task ShouldLoginWithFailureWrongPassword()
    {
        const string password = "Nuages123*$";
        
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);
        
        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            new Mock<IMessageService>().Object);

        var res = await loginService.LoginAsync(new LoginModel
        {
            UserNameOrEmail = user.Email,
            Password = "bad_password",
            RememberMe = false
        });
        
        Assert.False(res.Success);
        Assert.Equal(FailedLoginReason.UserNameOrPasswordInvalid, res.Reason);
        Assert.Equal(SignInResult.Failed, res.Result);
        Assert.NotStrictEqual("errorMessage:userNameOrPasswordInvalid", res.Message);
    }
    
    [Fact]
    public async Task ShouldLoginWithFailureWrongUsername()
    {
        const string password = "Nuages123*$";
        
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);
        
        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            messageService.Object);

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
        const string password = "Nuages123*$";
        
        var user = MockHelpers.CreateDefaultUser();
        user.AccessFailedCount = 4;
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.Now.AddDays(1);
         
      
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);
        
        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            messageService.Object);

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
    public async Task ShouldLoginWithFailureBadPasswordThenLockedOut()
    {
        const string password = "Nuages123*$";
        
        var user = MockHelpers.CreateDefaultUser();
        user.AccessFailedCount = 5;
        user.LockoutEnabled = true;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);
        
        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            new Mock<IMessageService>().Object);

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

        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            new Mock<IMessageService>().Object);
    
        var res = await loginService.Login2FAAsync(new Login2FAModel
        {
            Code = "ok",
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
        
        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            new Mock<IMessageService>().Object);
    
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
        
        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            new Mock<IMessageService>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await loginService.Login2FAAsync(new Login2FAModel
            {
                Code = "ok",
                RememberMachine = false,
                RememberMe = false
            });
            
        });
        
    }
    
    [Fact]
    public async Task ShoudLoginRecoveryCodeWithSuccess()
    {
        const string code = "123456";
        
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        
        var messageService = new Mock<IMessageService>();
    
        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            messageService.Object);

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
        
        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            new Mock<IMessageService>().Object);

        var res = await loginService.LoginRecoveryCodeAsync(new LoginRecoveryCodeModel
        {
            Code = "654321" //Bad code
        });
        
        Assert.False(res.Success);
    }

    
    [Fact]
    public async Task ShoudLoginRecoveryCodeWithFailuerUserDoesNotExists()
    {
        const string code = "123456";
        
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.SignInManager.CurrentUser = null;
        
        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            new Mock<IMessageService>().Object);

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
                 
        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            new Mock<IMessageService>().Object);

        var res = await loginService.LoginSMSAsync(new LoginSMSModel
        {
            Code = "123456"
        });
        
        Assert.True(res.Success);
    }
    
    [Fact]
    public async Task ShoudLoginSmsWithFailureUserNotFound()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.SignInManager.CurrentUser = null;
        
        var loginService = new LoginService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            new Mock<IMessageService>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await loginService.LoginSMSAsync(new LoginSMSModel
            {
                Code = "123456"
            });
        });
    }
    

}