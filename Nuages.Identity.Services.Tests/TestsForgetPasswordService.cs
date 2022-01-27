using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Password;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsForgetPasswordService
{
    [Fact]
    public async Task ShouldStartForgetPasswordWithSuccess()
    {
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "TEST@NUAGES.ORG",
            NormalizedEmail = "TEST@NUAGES.ORG",
            EmailConfirmed = true
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);
        
        bool sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var forgetPasswordService = new ForgotPasswordService(identityStuff.UserManager, MockHelpers.MockHttpContextAccessor().Object, messageService.Object, Options.Create(options));

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
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "TEST@NUAGES.ORG",
            NormalizedEmail = "TEST@NUAGES.ORG",
            EmailConfirmed = false
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);
        
        bool sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var forgetPasswordService = new ForgotPasswordService(identityStuff.UserManager, MockHelpers.MockHttpContextAccessor().Object, messageService.Object, Options.Create(options));

        var res = await forgetPasswordService.StartForgotPassword(new ForgotPasswordModel
        {
            Email = user.Email
        });
        
        Assert.True(res.Success);
        Assert.False(sendCalled);
    }
}