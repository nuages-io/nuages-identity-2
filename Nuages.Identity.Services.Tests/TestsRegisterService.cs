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
        const string email = MockHelpers.TestEmail;
        const string password = MockHelpers.StrongPassword;

        var identityStuff = MockHelpers.MockIdentityStuff(null);
        
        identityStuff.UserStore.Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>IdentityResult.Success);
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var registerService = new RegisterService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            messageService.Object, Options.Create(identityStuff.NuagesOptions));

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
        const string email = MockHelpers.TestEmail;
        const string password = MockHelpers.StrongPassword;
        

        var options = new NuagesIdentityOptions
        {
            RequireConfirmedEmail = true
        };

        var identityStuff = MockHelpers.MockIdentityStuff(null, options);
        
        identityStuff.UserStore.Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>IdentityResult.Success);
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var registerService = new RegisterService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
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
        const string password = MockHelpers.StrongPassword;

        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        
        identityStuff.UserStore.Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>IdentityResult.Success);
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var registerService = new RegisterService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            messageService.Object, Options.Create(identityStuff.NuagesOptions));

        var res = await registerService.Register(new RegisterModel
        {
            Email = user.Email,
            Password = password,
            PasswordConfirm = password
        });
        
        Assert.False(res.Success);
        Assert.Equal("register.userEmailAlreadyExists", res.Errors.Single());
        Assert.False(sendCalled);
    }
    
    [Fact]
    public async Task ShouldRegisterUserWithErrorPasswordDoesNotMatch()
    {
        const string password = MockHelpers.StrongPassword;

        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        
        identityStuff.UserStore.Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>IdentityResult.Success);
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var registerService = new RegisterService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            messageService.Object, Options.Create(identityStuff.NuagesOptions));

        var res = await registerService.Register(new RegisterModel
        {
            Email = user.Email,
            Password = password,
            PasswordConfirm = "bad_password"
        });
        
        Assert.False(res.Success);
        Assert.Equal("register.passwordDoesNotMatch", res.Errors.Single());
        Assert.False(sendCalled);
    }
    
    [Fact]
    public async Task ShouldRegisterUserWithErrorPasswordNotStrongEnough()
    {
        const string password = "password";
        const string email = MockHelpers.TestEmail;

        var identityStuff = MockHelpers.MockIdentityStuff(null);
        
        identityStuff.UserStore.Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>IdentityResult.Success);
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var registerService = new RegisterService(identityStuff.UserManager, identityStuff.SignInManager, new FakeStringLocalizer(),
            messageService.Object, Options.Create(identityStuff.NuagesOptions));

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