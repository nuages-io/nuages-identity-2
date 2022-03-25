using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Identity.Services.Password;
using Nuages.Web;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsForgetPasswordService
{
    [Fact]
    public async Task ShouldStartForgetPasswordWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        user.EmailConfirmed = true;

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var sendCalled = false;

        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var forgetPasswordService = new ForgotPasswordService(identityStuff.UserManager,
            MockHelpers.MockHttpContextAccessor().Object, messageService.Object,
            Options.Create(identityStuff.NuagesOptions), new Mock<IRuntimeConfiguration>().Object);

        var res = await forgetPasswordService.StartForgotPassword(new ForgotPasswordModel
        {
            Email = user.Email
        });

        Assert.True(res.Success);
        Assert.True(sendCalled);
    }

    [Fact]
    public async Task ShouldStartForgetPasswordWithSuccessEmailNotSentBecauseNotCOnfirmed()
    {
        var user = MockHelpers.CreateDefaultUser();
        user.EmailConfirmed = false;

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var sendCalled = false;

        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var forgetPasswordService = new ForgotPasswordService(identityStuff.UserManager,
            MockHelpers.MockHttpContextAccessor().Object, messageService.Object,
            Options.Create(identityStuff.NuagesOptions), new Mock<IRuntimeConfiguration>().Object);

        var res = await forgetPasswordService.StartForgotPassword(new ForgotPasswordModel
        {
            Email = user.Email
        });

        Assert.True(res.Success);
        Assert.False(sendCalled);
    }
}