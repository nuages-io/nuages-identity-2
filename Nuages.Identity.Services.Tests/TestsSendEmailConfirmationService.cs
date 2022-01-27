using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsSendEmailConfirmationService
{
    [Fact]
    public async Task SendEmailConfirmationWithSuccess()
    {
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@nuages.org",
            NormalizedEmail = "TEST@NUAGES.ORG"
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);

        bool sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var service = new SendEmailConfirmationService(identityStuff.UserManager, messageService.Object, Options.Create(options));

        var res = await service.SendEmailConfirmation(new SendEmailConfirmationModel
        {
            Email = user.Email
        });
        
        Assert.True(res.Success);
        Assert.True(sendCalled);
    }
    
    [Fact]
    public async Task SendEmailConfirmationWithSuccessButMEssageNotSent()
    {
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@nuages.org",
            NormalizedEmail = "TEST@NUAGES.ORG"
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user,  options);

        bool sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var service = new SendEmailConfirmationService(identityStuff.UserManager, messageService.Object, Options.Create(options));

        var res = await service.SendEmailConfirmation(new SendEmailConfirmationModel
        {
            Email = "bademail@nuages.org"
        });
        
        Assert.True(res.Success);
        Assert.True(!sendCalled);
    }
}