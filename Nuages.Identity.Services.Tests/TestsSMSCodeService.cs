using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Login;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsSMSCodeService
{
    [Fact]
    public async Task ShouldSendCodeWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        user.PhoneNumber = "9999999999";
        user.PhoneNumberConfirmed = true;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendSms(user.PhoneNumber, It.IsAny<string>()))
            .Callback(() => sendCalled = true);

        var service = new SMSCodeService(identityStuff.UserManager, identityStuff.SignInManager, messageService.Object,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(),
            new Mock<ILogger<SMSCodeService>>().Object);

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

        var service = new SMSCodeService(identityStuff.UserManager, identityStuff.SignInManager, messageService.Object,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(),
            new Mock<ILogger<SMSCodeService>>().Object);

        var res = await service.SendCode();
        
        Assert.True(res.Success);
        Assert.False(sendCalled);

    }
    
    [Fact]
    public async Task ShouldSendCodeWithExceptionInvalidOperation()
    {
        var user = MockHelpers.CreateDefaultUser();
        user.PhoneNumber = "9999999999";
        user.PhoneNumberConfirmed = true;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var messageService = new Mock<IMessageService>();

        var service = new SMSCodeService(identityStuff.UserManager, identityStuff.SignInManager, messageService.Object,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(),
            new Mock<ILogger<SMSCodeService>>().Object);

        identityStuff.SignInManager.CurrentUser = null;
        
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.SendCode();
        });

    }
    
    [Fact]
    public async Task ShouldSendCodeWithExceptionNotFound()
    {
        var user = MockHelpers.CreateDefaultUser();
        user.PhoneNumber = "9999999999";
        user.PhoneNumberConfirmed = true;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var messageService = new Mock<IMessageService>();

        var service = new SMSCodeService(identityStuff.UserManager, identityStuff.SignInManager, messageService.Object,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(),
            new Mock<ILogger<SMSCodeService>>().Object);

        identityStuff.SignInManager.CurrentUser = null;
        
        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await service.SendCode("bad_id");
        });

    }
}