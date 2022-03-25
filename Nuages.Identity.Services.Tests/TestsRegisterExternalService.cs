using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Identity.Services.Register;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsRegisterExternalService
{
    [Fact]
    public async Task ShouldRegisterWithSuccess()
    {
        const string email = MockHelpers.TestEmail;

        var identityStuff = MockHelpers.MockIdentityStuff(null);
        identityStuff.SignInManager.CurrentUser = new NuagesApplicationUser<string>
        {
            Email = email,
            EmailConfirmed = false
        };

        identityStuff.UserManager.Options.SignIn.RequireConfirmedEmail = false;

        identityStuff.UserStore
            .Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => IdentityResult.Success);

        var sendCalled = false;

        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var registerService = new RegisterExternalLoginService(identityStuff.SignInManager, identityStuff.UserManager,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(), messageService.Object);

        var res = await registerService.Register();

        Assert.True(res.Success);
        Assert.False(sendCalled);
    }

    [Fact]
    public async Task ShouldRegisterWithSuccessRequireConfirmation()
    {
        const string email = MockHelpers.TestEmail;

        var identityStuff = MockHelpers.MockIdentityStuff(null);
        identityStuff.SignInManager.CurrentUser = new NuagesApplicationUser<string>
        {
            Email = email,
            EmailConfirmed = false
        };

        identityStuff.UserManager.Options.SignIn.RequireConfirmedEmail = true;

        identityStuff.UserStore
            .Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => IdentityResult.Success);

        var sendCalled = false;

        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);

        var registerService = new RegisterExternalLoginService(identityStuff.SignInManager, identityStuff.UserManager,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(), messageService.Object);

        var res = await registerService.Register();

        Assert.True(res.Success);
        Assert.True(sendCalled);
    }

    [Fact]
    public async Task ShouldRegisterWithErrorNoEmailClaim()
    {
        var identityStuff = MockHelpers.MockIdentityStuff(null);
        identityStuff.SignInManager.CurrentUser = new NuagesApplicationUser<string>
        {
            Email = "" //NO EMAIL
        };

        identityStuff.UserManager.Options.SignIn.RequireConfirmedEmail = false;

        identityStuff.UserStore
            .Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => IdentityResult.Success);


        var registerService = new RegisterExternalLoginService(identityStuff.SignInManager, identityStuff.UserManager,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(), new Mock<IMessageService>().Object);

        var res = await registerService.Register();

        Assert.False(res.Success);
    }

    [Fact]
    public async Task ShouldRegisterWithErrorNoUser()
    {
        var identityStuff = MockHelpers.MockIdentityStuff(null);

        identityStuff.UserManager.Options.SignIn.RequireConfirmedEmail = false;

        identityStuff.UserStore
            .Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => IdentityResult.Success);

        var registerService = new RegisterExternalLoginService(identityStuff.SignInManager, identityStuff.UserManager,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(), new Mock<IMessageService>().Object);

        var res = await registerService.Register();

        Assert.False(res.Success);
    }


    [Fact]
    public async Task ShouldRegisterWithErrorCreateFailed()
    {
        const string email = MockHelpers.TestEmail;

        var identityStuff = MockHelpers.MockIdentityStuff(null);
        identityStuff.SignInManager.CurrentUser = new NuagesApplicationUser<string>
        {
            Email = email,
            EmailConfirmed = false
        };

        var validator = new Mock<IUserValidator<NuagesApplicationUser<string>>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<NuagesUserManager>(), It.IsAny<NuagesApplicationUser<string>>()))
            .ReturnsAsync(() => IdentityResult.Failed(new IdentityError
            {
                Description = "error"
            }));

        identityStuff.UserManager.UserValidators.Add(validator.Object);
        identityStuff.UserManager.Options.SignIn.RequireConfirmedEmail = false;

        identityStuff.UserStore
            .Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => IdentityResult.Success);

        var registerService = new RegisterExternalLoginService(identityStuff.SignInManager, identityStuff.UserManager,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(), new Mock<IMessageService>().Object);

        var res = await registerService.Register();

        Assert.False(res.Success);
    }

    [Fact]
    public async Task ShouldRegisterWithErrorCantAddLogin()
    {
        const string email = "INVALID@NUAGES.ORG";

        var identityStuff = MockHelpers.MockIdentityStuff(null);
        identityStuff.SignInManager.CurrentUser = new NuagesApplicationUser<string>
        {
            Email = email,
            EmailConfirmed = false
        };

        identityStuff.UserStore
            .Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => IdentityResult.Success);

        var registerService = new RegisterExternalLoginService(identityStuff.SignInManager, identityStuff.UserManager,
            Options.Create(identityStuff.NuagesOptions), new FakeStringLocalizer(), new Mock<IMessageService>().Object);

        var res = await registerService.Register();

        Assert.False(res.Success);
    }
}