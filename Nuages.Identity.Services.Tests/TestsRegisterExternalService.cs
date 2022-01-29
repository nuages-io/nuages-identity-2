using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Register;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsRegisterExternalService
{
    [Fact]
    public async Task ShouldRegisterWithSuccess()
    {
        const string email = "TEST@NUAGES.ORG";
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(null, options);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(options))
         {
             CurrentUser = new NuagesApplicationUser
             {
                 Email = email
             }
         };

        identityStuff.UserStore.Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>IdentityResult.Success);
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var registerService = new RegisterExternalLoginService(fakeSignInManager, identityStuff.UserManager,
            Options.Create(options), new FakeStringLocalizer(), messageService.Object);

        var res = await registerService.Register();
        
        Assert.True(res.Success);
        Assert.True(sendCalled);
    }
}