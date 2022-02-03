using Microsoft.Extensions.Options;
using Moq;
using Nuages.AspNetIdentity;

using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Manage;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsSendEmailChangeConfirmationService
{
    [Fact]
    public async Task ShouldSendSendEMailWithSuccess()
    {
        const string newEmail = MockHelpers.NewTestEmail;
        
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(newEmail, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);


        var service = new SendEmailChangeConfirmationService(identityStuff.UserManager, messageService.Object,
          Options.Create(identityStuff.NuagesOptions)  , new FakeStringLocalizer());

        var res = await service.SendEmailChangeConfirmation(user.Id, newEmail);
        
        Assert.True(res.Success);
        Assert.True(sendCalled);
    }
    
    [Fact]
    public async Task ShouldSendSendEMailWithErrorNotChanged()
    {

        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var service = new SendEmailChangeConfirmationService(identityStuff.UserManager, messageService.Object,
            Options.Create(identityStuff.NuagesOptions)  , new FakeStringLocalizer());

        var res = await service.SendEmailChangeConfirmation(user.Id, user.Email);
        
        Assert.False(res.Success);
        Assert.Equal("changeEmail:isNotChanged", res.Errors.Single());
        Assert.False(sendCalled);
    }
    
    [Fact]
    public async Task ShouldSendSendEMailWithErrorAlreadyExists()
    {
        var newEmail = MockHelpers.NewTestEmail.ToUpper();
        
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.UserEmailStore.Setup(u => 
            u.FindByEmailAsync(newEmail, It.IsAny<CancellationToken>())).ReturnsAsync( () => new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString()
        });
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(newEmail, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);


        var service = new SendEmailChangeConfirmationService(identityStuff.UserManager, messageService.Object,
            Options.Create(identityStuff.NuagesOptions)  , new FakeStringLocalizer());

        var res = await service.SendEmailChangeConfirmation(user.Id, newEmail);
        
        Assert.False(res.Success);
        Assert.Equal("changeEmail:emailAlreadyUsed", res.Errors.Single());
        Assert.False(sendCalled);
    }
    
    [Fact]
    public async Task ShouldSendSendEMailWithExceptionNotFound()
    {

        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        
        var service = new SendEmailChangeConfirmationService(identityStuff.UserManager, new Mock<IMessageService>().Object,
            Options.Create(identityStuff.NuagesOptions)  , new FakeStringLocalizer());

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await service.SendEmailChangeConfirmation(MockHelpers.BadId, user.Email);
        });
    }
}