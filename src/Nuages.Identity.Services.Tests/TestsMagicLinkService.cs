using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Identity.Services.Login.MagicLink;
using Nuages.Web;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsMagicLinkService
{
    [Fact]
    public async Task ShoudGetMagicLinkUrlWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        identityStuff.NuagesOptions.Authority = "https://localhost:8001/";
        
        var service = new MagicLinkService(identityStuff.UserManager, identityStuff.SignInManager,
            new Mock<IMessageService>().Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions),
            new Mock<IRuntimeConfiguration>().Object, new Mock<IIdentityEventBus>().Object);

        var url = await service.GetMagicLinkUrl(user.Id);


        Assert.NotNull(url);
    }

    [Fact]
    public async Task ShoudGetMagicLinkUrlWithExceptionNotFound()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var service = new MagicLinkService(identityStuff.UserManager, identityStuff.SignInManager,
            new Mock<IMessageService>().Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions),
            new Mock<IRuntimeConfiguration>().Object, new Mock<IIdentityEventBus>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await service.GetMagicLinkUrl(MockHelpers.BadId);
        });
    }

    [Fact]
    public async Task ShoudStartMagicLinkWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        identityStuff.NuagesOptions.Authority = "https://localhost:8001/";
        var sendCalled = false;

        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var service = new MagicLinkService(identityStuff.UserManager, identityStuff.SignInManager,
            messageService.Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions),
            new Mock<IRuntimeConfiguration>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await service.StartMagicLink(new StartMagicLinkModel
        {
            Email = user.Email
        });

        Assert.True(res.Success);
        Assert.True(sendCalled);
    }

    [Fact]
    public async Task ShoudStartMagicLinkWithSuccessEmailNotFOund()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var sendCalled = false;

        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var service = new MagicLinkService(identityStuff.UserManager, identityStuff.SignInManager,
            messageService.Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions),
            new Mock<IRuntimeConfiguration>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await service.StartMagicLink(new StartMagicLinkModel
        {
            Email = "invalid@nuages.org"
        });

        Assert.True(res.Success);
        Assert.False(sendCalled);
    }

    [Fact]
    public async Task ShoudStartMagicLinkWithErrorCantLogin()
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

        var service = new MagicLinkService(identityStuff.UserManager, identityStuff.SignInManager,
            messageService.Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions),
            new Mock<IRuntimeConfiguration>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await service.StartMagicLink(new StartMagicLinkModel
        {
            Email = user.Email
        });

        Assert.False(res.Success);
        Assert.Equal("errorMessage:no_access:LockedOut", res.Message);
        Assert.Equal(FailedLoginReason.LockedOut, res.Reason);
        Assert.Equal(SignInResult.LockedOut, res.Result);

        Assert.False(sendCalled);
    }

    [Fact]
    public async Task ShoudLoginMagicLinkWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var service = new MagicLinkService(identityStuff.UserManager, identityStuff.SignInManager,
            new Mock<IMessageService>().Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions),
            new Mock<IRuntimeConfiguration>().Object, new Mock<IIdentityEventBus>().Object);

        var token = await identityStuff.UserManager.GenerateUserTokenAsync(user, "MagicLinkLoginProvider",
            "magiclink-auth");

        var url = await service.LoginMagicLink(token, user.Id);

        Assert.True(url.Success);
    }

    [Fact]
    public async Task ShoudLoginMagicLinkWithErrorBadToken()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var service = new MagicLinkService(identityStuff.UserManager, identityStuff.SignInManager,
            new Mock<IMessageService>().Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions),
            new Mock<IRuntimeConfiguration>().Object, new Mock<IIdentityEventBus>().Object);


        var url = await service.LoginMagicLink("bad_token", user.Id);

        Assert.False(url.Success);
    }

    [Fact]
    public async Task ShoudLoginMagicLinkWithErrorCantLogin()
    {
        var user = MockHelpers.CreateDefaultUser();
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.Now.AddMinutes(10);

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var service = new MagicLinkService(identityStuff.UserManager, identityStuff.SignInManager,
            new Mock<IMessageService>().Object, new FakeStringLocalizer(), Options.Create(identityStuff.NuagesOptions),
            new Mock<IRuntimeConfiguration>().Object, new Mock<IIdentityEventBus>().Object);

        var token = await identityStuff.UserManager.GenerateUserTokenAsync(user, "MagicLinkLoginProvider",
            "magiclink-auth");

        var res = await service.LoginMagicLink(token, user.Id);

        Assert.False(res.Success);
        Assert.Equal("errorMessage:no_access:LockedOut", res.Message);
        Assert.Equal(FailedLoginReason.LockedOut, res.Reason);
        Assert.Equal(SignInResult.LockedOut, res.Result);
    }
}