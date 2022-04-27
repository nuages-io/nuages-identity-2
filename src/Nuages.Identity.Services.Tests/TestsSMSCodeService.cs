using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Identity.Services.Login;
using Nuages.Web;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsSmsCodeService
{
    [Fact]
    public async Task ShouldSendCodeWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        user.PhoneNumber = MockHelpers.PhoneNumber;
        user.PhoneNumberConfirmed = true;

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var sendCalled = false;

        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendSms(user.PhoneNumber, It.IsAny<string>()))
            .Callback(() => sendCalled = true);

        var service = new SMSSendCodeService(identityStuff.UserManager, identityStuff.SignInManager,
            messageService.Object,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(),
            new Mock<ILogger<SMSSendCodeService>>().Object, new Mock<IRuntimeConfiguration>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await service.SendCode();

        Assert.True(res.Success);
        Assert.True(sendCalled);
    }

    [Fact]
    public async Task ShouldSendCodeWithSuccessWithoutPhoneNotSent()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var sendCalled = false;

        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendSms(user.PhoneNumber, It.IsAny<string>()))
            .Callback(() => sendCalled = true);

        var service = new SMSSendCodeService(identityStuff.UserManager, identityStuff.SignInManager,
            messageService.Object,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(),
            new Mock<ILogger<SMSSendCodeService>>().Object, new Mock<IRuntimeConfiguration>().Object, new Mock<IIdentityEventBus>().Object);

        var res = await service.SendCode();

        Assert.True(res.Success);
        Assert.False(sendCalled);
    }

    [Fact]
    public async Task ShouldSendCodeWithExceptionInvalidOperation()
    {
        var user = MockHelpers.CreateDefaultUser();
        user.PhoneNumber = MockHelpers.PhoneNumber;
        user.PhoneNumberConfirmed = true;

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var messageService = new Mock<IMessageService>();

        var service = new SMSSendCodeService(identityStuff.UserManager, identityStuff.SignInManager,
            messageService.Object,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(),
            new Mock<ILogger<SMSSendCodeService>>().Object, new Mock<IRuntimeConfiguration>().Object, new Mock<IIdentityEventBus>().Object);

        identityStuff.SignInManager.CurrentUser = null;

        await Assert.ThrowsAsync<InvalidOperationException>(async () => { await service.SendCode(); });
    }

    [Fact]
    public async Task ShouldSendCodeWithExceptionNotFound()
    {
        var user = MockHelpers.CreateDefaultUser();
        user.PhoneNumber = MockHelpers.PhoneNumber;
        user.PhoneNumberConfirmed = true;

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var messageService = new Mock<IMessageService>();

        var service = new SMSSendCodeService(identityStuff.UserManager, identityStuff.SignInManager,
            messageService.Object,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(),
            new Mock<ILogger<SMSSendCodeService>>().Object, new Mock<IRuntimeConfiguration>().Object, new Mock<IIdentityEventBus>().Object);

        identityStuff.SignInManager.CurrentUser = null;

        await Assert.ThrowsAsync<NotFoundException>(async () => { await service.SendCode(MockHelpers.BadId); });
    }
}