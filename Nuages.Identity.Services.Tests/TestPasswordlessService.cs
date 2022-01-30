using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Login.Passwordless;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestPasswordlessService
{
    [Fact]
    public async Task ShoudGetPasswordLessUrlWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var service = new PasswordlessService(identityStuff.UserManager, identityStuff.SignInManager,
            new Mock<IMessageService>().Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions));

        var url = await service.GetPasswordlessUrl(user.Id);
        
        Assert.True(url.Success);
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
            await service.GetPasswordlessUrl("bad_id");
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
        user.EmailConfirmed = false;
        user.LockoutEnd = DateTimeOffset.Now.AddMinutes(10);
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        //identityStuff.UserManager.Options.SignIn.RequireConfirmedEmail = true;
        
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
        Assert.False(sendCalled);
    }

}