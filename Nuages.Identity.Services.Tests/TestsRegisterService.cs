using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Register;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsRegisterService
{
    [Fact]
    public async Task ShouldRegisterUserWithSuccess()
    {
        var email = "test@nuages.org";
        var password = "Password123*#";

        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(null, options);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(options));
        
        identityStuff.UserStore.Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>IdentityResult.Success);
        
        bool sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var registerService = new RegisterService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
            messageService.Object, Options.Create(options));

        var res = await registerService.Register(new RegisterModel
        {
            Email = email,
            Password = password,
            PasswordConfirm = password
        });
        
        Assert.True(res.Success);
        Assert.False(res.ShowConfirmationMessage);
        Assert.True(sendCalled);
    }
    
    [Fact]
    public async Task ShouldRegisterUserWithSuccessDoesNotLogin()
    {
        var email = "test@nuages.org";
        var password = "Password123*#";

        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email
        };

        var options = new NuagesIdentityOptions
        {
            RequireConfirmedEmail = true
        };

        var identityStuff = MockHelpers.MockIdentityStuff(null, options);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(options));
        
        identityStuff.UserStore.Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>IdentityResult.Success);
        
        bool sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var registerService = new RegisterService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
            messageService.Object, Options.Create(options));

        var res = await registerService.Register(new RegisterModel
        {
            Email = email,
            Password = password,
            PasswordConfirm = password
        });
        
        Assert.True(res.Success);
        Assert.True(res.ShowConfirmationMessage);
        Assert.True(sendCalled);
    }
    
    [Fact]
    public async Task ShouldRegisterUserWithErrorAlreadyExists()
    {
        var password = "Password123*#";

        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@nuages.org",
            NormalizedEmail = "TEST@NUAGES.ORG"
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(options));
        
        identityStuff.UserStore.Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>IdentityResult.Success);
        
        bool sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var registerService = new RegisterService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
            messageService.Object, Options.Create(options));

        var res = await registerService.Register(new RegisterModel
        {
            Email = user.Email,
            Password = password,
            PasswordConfirm = password
        });
        
        Assert.False(res.Success);
        Assert.Equal("register.userEmailAlreadyExists", res.Errors.First());
        Assert.False(sendCalled);
    }
    
    [Fact]
    public async Task ShouldRegisterUserWithErrorPasswordDoesNotMatch()
    {
        var password = "Password123*#";

        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@nuages.org",
            NormalizedEmail = "TEST@NUAGES.ORG"
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(options));
        
        identityStuff.UserStore.Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>IdentityResult.Success);
        
        bool sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var registerService = new RegisterService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
            messageService.Object, Options.Create(options));

        var res = await registerService.Register(new RegisterModel
        {
            Email = user.Email,
            Password = password,
            PasswordConfirm = "bad_password"
        });
        
        Assert.False(res.Success);
        Assert.Equal("register.passwordDoesNotMatch", res.Errors.First());
        Assert.False(sendCalled);
    }
    
    [Fact]
    public async Task ShouldRegisterUserWithErrorPasswordNotStrongEnough()
    {
        var password = "password";
        var email = "test@nuages.org";

        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(null, options);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(options));
        
        identityStuff.UserStore.Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>IdentityResult.Success);
        
        bool sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var registerService = new RegisterService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
            messageService.Object, Options.Create(options));

        var res = await registerService.Register(new RegisterModel
        {
            Email = email,
            Password = password,
            PasswordConfirm = password
        });
        
        Assert.False(res.Success);
        Assert.Equal("identity.PasswordRequiresNonAlphanumeric", res.Errors[0]);
        Assert.Equal("identity.PasswordRequiresDigit", res.Errors[1]);
        Assert.Equal("identity.PasswordRequiresUpper", res.Errors[2]);
        Assert.False(sendCalled);
    }
}