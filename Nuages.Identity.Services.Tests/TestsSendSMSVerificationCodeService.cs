using Microsoft.Extensions.Logging;
using Moq;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Manage;
using Nuages.Web;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsSendSmsVerificationCodeService
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

        var service =
            new SendSMSVerificationCodeService(identityStuff.UserManager, messageService.Object,
                new FakeStringLocalizer(), new Mock<ILogger<SendSMSVerificationCodeService>>().Object,
                new Mock<IRuntimeConfiguration>().Object);

        var res = await service.SendCode(user.Id, user.PhoneNumber);

        Assert.True(res.Success);
        Assert.True(sendCalled);
    }

    [Fact]
    public async Task ShouldSendCodeExceptionNotFound()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var service =
            new SendSMSVerificationCodeService(identityStuff.UserManager, new Mock<IMessageService>().Object,
                new FakeStringLocalizer(), new Mock<ILogger<SendSMSVerificationCodeService>>().Object,
                new Mock<IRuntimeConfiguration>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await service.SendCode(MockHelpers.BadId, MockHelpers.PhoneNumber);
        });
    }
}