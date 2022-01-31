using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Login;
using Nuages.Identity.Services.Login.Passwordless;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsPasswordlessService
{
    [Fact]
    public async Task ShoudGetPasswordLessUrlWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var service = new PasswordlessService(identityStuff.UserManager, identityStuff.SignInManager,
            new Mock<IMessageService>().Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions));

        var url = await service.GetPasswordlessUrl(user.Id);
        
        
        Assert.NotNull(url);
    }
    
    [Fact]
    public async Task ShoudGetPasswordLessUrlWithExceptionNotFound()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var service = new PasswordlessService(identityStuff.UserManager, identityStuff.SignInManager,
            new Mock<IMessageService>().Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions));

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await service.GetPasswordlessUrl(MockHelpers.BadId);
        });
        
    }
    
    [Fact]
    public async Task ShoudStartPasswordlessWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var service = new PasswordlessService(identityStuff.UserManager, identityStuff.SignInManager,
            messageService.Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions));

        var res = await service.StartPasswordless(new StartPasswordlessModel
        {
            Email = user.Email
        });

        Assert.True(res.Success);
        Assert.True(sendCalled);
    }
    
    [Fact]
    public async Task ShoudStartPasswordlessWithSuccessEmailNotFOund()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var service = new PasswordlessService(identityStuff.UserManager, identityStuff.SignInManager,
            messageService.Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions));

        var res = await service.StartPasswordless(new StartPasswordlessModel
        {
            Email = "invalid@nuages.org"
        });

        Assert.True(res.Success);
        Assert.False(sendCalled);
    }
    
    [Fact]
    public async Task ShoudStartPasswordlessWithErrorCantLogin()
    {
        var user = MockHelpers.CreateDefaultUser();
      
        user.LockoutEnd = DateTimeOffset.Now.AddMinutes(10);
        user.LockoutEnabled = true;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var service = new PasswordlessService(identityStuff.UserManager, identityStuff.SignInManager,
            messageService.Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions));

        var res = await service.StartPasswordless(new StartPasswordlessModel
        {
            Email = user.Email
        });

        Assert.False(res.Success);
        Assert.Equal("errorMessage:no_access:LockedOut", res.Message);
        Assert.Equal(FailedLoginReason.LockedOut, res.Reason);
        Assert.Equal(Microsoft.AspNetCore.Identity.SignInResult.LockedOut, res.Result);
        
        Assert.False(sendCalled);
    }

    [Fact]
    public async Task ShoudLoginPasswordlessWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var service = new PasswordlessService(identityStuff.UserManager, identityStuff.SignInManager,
            new Mock<IMessageService>().Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions));

        var token = await identityStuff.UserManager.GenerateUserTokenAsync(user, "PasswordlessLoginProvider",
            "passwordless-auth");
        
        var url = await service.LoginPasswordLess(token, user.Id);
        
        Assert.True(url.Success);
    }
    
    [Fact]
    public async Task ShoudLoginPasswordlessWithErrorBadToken()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var service = new PasswordlessService(identityStuff.UserManager, identityStuff.SignInManager,
            new Mock<IMessageService>().Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions));

        
        var url = await service.LoginPasswordLess("bad_token", user.Id);
        
        Assert.False(url.Success);
    }
    
    [Fact]
    public async Task ShoudLoginPasswordlessWithErrorCantLogin()
    {
        var user = MockHelpers.CreateDefaultUser();
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.Now.AddMinutes(10);
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var service = new PasswordlessService(identityStuff.UserManager, identityStuff.SignInManager,
            new Mock<IMessageService>().Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions));

        var token = await identityStuff.UserManager.GenerateUserTokenAsync(user, "PasswordlessLoginProvider",
            "passwordless-auth");
        
        var res = await service.LoginPasswordLess(token, user.Id);
        
        Assert.False(res.Success);
        Assert.Equal("errorMessage:no_access:LockedOut", res.Message);
        Assert.Equal(FailedLoginReason.LockedOut, res.Reason);
        Assert.Equal(Microsoft.AspNetCore.Identity.SignInResult.LockedOut, res.Result);
    }

}