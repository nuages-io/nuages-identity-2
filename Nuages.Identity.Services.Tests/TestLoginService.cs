using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Login;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestLoginService
{
    [Fact]
    public async Task ShouldLoginWithSuccess()
    {
        const string email = "TEST@NUAGES.ORG";
        const string password = "Nuages123*$";
        
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email,
            UserName = email,
            NormalizedUserName = email,
            EmailConfirmed = true
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(options));
        
        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
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
        const string email = "TEST@NUAGES.ORG";
        const string password = "Nuages123*$";
        
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email,
            UserName = email,
            NormalizedUserName = email,
            EmailConfirmed = true
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(options));
        
        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
            messageService.Object);

        var res = await loginService.LoginAsync(new LoginModel
        {
            UserNameOrEmail = user.Email,
            Password = "bad_password",
            RememberMe = false
        });
        
        Assert.False(res.Success);
        Assert.Equal(FailedLoginReason.UserNameOrPasswordInvalid, res.Reason);
    }
    
    [Fact]
    public async Task ShouldLoginWithFailureWrongUsername()
    {
        const string email = "TEST@NUAGES.ORG";
        const string password = "Nuages123*$";
        
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email,
            UserName = email,
            NormalizedUserName = email,
            EmailConfirmed = true
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(options));
        
        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
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
        const string email = "TEST@NUAGES.ORG";
        const string password = "Nuages123*$";
        
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email,
            UserName = email,
            NormalizedUserName = email,
            EmailConfirmed = true,
            AccessFailedCount = 4,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.Now.AddDays(1)
        };
        
        var options = new NuagesIdentityOptions();
    
        var identityStuff = MockHelpers.MockIdentityStuff(user, options);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(options));
        
        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
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
        const string email = "TEST@NUAGES.ORG";
        const string password = "Nuages123*$";
        
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email,
            UserName = email,
            NormalizedUserName = email,
            EmailConfirmed = true,
            AccessFailedCount = 5,
            LockoutEnabled = true,
            LockoutEnd = null
        };
        
        var options = new NuagesIdentityOptions();
    
        var identityStuff = MockHelpers.MockIdentityStuff(user, options);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(new NuagesIdentityOptions()));
        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
            messageService.Object);

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
        const string email = "TEST@NUAGES.ORG";

        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email,
            UserName = email,
            NormalizedUserName = email,
            EmailConfirmed = true,
            AccessFailedCount = 5,
            LockoutEnabled = true,
            LockoutEnd = null
        };
        
        var options = new NuagesIdentityOptions();
    
        var identityStuff = MockHelpers.MockIdentityStuff(user, options);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(new NuagesIdentityOptions()))
            {
                CurrentUser = user
            };

        var messageService = new Mock<IMessageService>();
    
        var loginService = new LoginService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
            messageService.Object);
    
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
        const string email = "TEST@NUAGES.ORG";

        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email,
            UserName = email,
            NormalizedUserName = email,
            EmailConfirmed = true,
            AccessFailedCount = 5,
            LockoutEnabled = true,
            LockoutEnd = null
        };
        
        var options = new NuagesIdentityOptions();
    
        var identityStuff = MockHelpers.MockIdentityStuff(user, options);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(new NuagesIdentityOptions()))
            {
                CurrentUser = user
            };

        var messageService = new Mock<IMessageService>();
    
        var loginService = new LoginService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
            messageService.Object);
    
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
        const string email = "TEST@NUAGES.ORG";

        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email,
            UserName = email,
            NormalizedUserName = email,
            EmailConfirmed = true,
            AccessFailedCount = 5,
            LockoutEnabled = true,
            LockoutEnd = null
        };
        
        var options = new NuagesIdentityOptions();
    
        var identityStuff = MockHelpers.MockIdentityStuff(user, options);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(new NuagesIdentityOptions()));
        //fakeSignInManager.CurrentUser = user;
        
        var messageService = new Mock<IMessageService>();
    
        var loginService = new LoginService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
            messageService.Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await loginService.Login2FAAsync(new Login2FAModel
            {
                Code = "bad_code",
                RememberMachine = false,
                RememberMe = false
            });
            
        });
        
    }
}