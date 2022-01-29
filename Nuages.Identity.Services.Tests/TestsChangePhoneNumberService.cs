using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Manage;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsChangePhoneNumberService
{
    [Fact]
    public async Task ChangePhoneNumberWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var phoneNumber = "9999999999";
        
        var token = await identityStuff.UserManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);

        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var changePhoneNumberService = new ChangePhoneNumberService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        var res = await changePhoneNumberService.ChangePhoneNumberAsync(user.Id, phoneNumber, token);
        
        Assert.True(res.Success);
        Assert.True(sendCalled);
    }
    
    [Fact]
    public async Task ChangePhoneNumberWithSuccessWithoutToken()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var phoneNumber = "9999999999";
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var changePhoneNumberService = new ChangePhoneNumberService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        var res = await changePhoneNumberService.ChangePhoneNumberAsync(user.Id, phoneNumber, null);
        
        Assert.True(res.Success);
        Assert.True(sendCalled);
    }
    
    [Fact]
    public async Task ChangePhoneNumberWithExceptionUserNotFOund()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var phoneNumber = "9999999999";
        
        var changePhoneNumberService = new ChangePhoneNumberService(identityStuff.UserManager, new FakeStringLocalizer(), new Mock<IMessageService>().Object);
        
        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await changePhoneNumberService.ChangePhoneNumberAsync("bad_id", phoneNumber, null);
        });
    }
    
    [Fact]
    public async Task ChangePhoneNumberEmptyWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var phoneNumber = "";
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var changePhoneNumberService = new ChangePhoneNumberService(identityStuff.UserManager, new FakeStringLocalizer(), messageService.Object);

        var res = await changePhoneNumberService.ChangePhoneNumberAsync(user.Id, phoneNumber, null);
        
        Assert.True(res.Success);
        Assert.True(sendCalled);
    }
}