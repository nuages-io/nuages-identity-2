using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Manage;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsMfaService
{
    [Fact]
    public async Task ShouldGetMfaUrlWithSucess()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        string? currentKey = null;

        identityStuff.UserAuthenticatorKeyStore.Setup(c =>
                c.GetAuthenticatorKeyAsync(It.IsAny<NuagesApplicationUser<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => currentKey);
        
        identityStuff.UserAuthenticatorKeyStore.Setup(c =>
                c.SetAuthenticatorKeyAsync(It.IsAny<NuagesApplicationUser<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback((NuagesApplicationUser<string> _, string key, CancellationToken _) => currentKey = key);
        
        var service = new MFAService(identityStuff.UserManager, UrlEncoder.Default, new FakeStringLocalizer(),
            Options.Create(identityStuff.NuagesOptions), new Mock<IMessageService>().Object);

        var res = await service.GetMFAUrlAsync(user.Id);
        
        Assert.True(res.Success);
        Assert.NotNull(res.Key);
        Assert.NotNull(res.Url);
    }
    
    [Fact]
    public async Task ShouldGetMfaUrlWithExceptionsNotFound()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        
        var service = new MFAService(identityStuff.UserManager, UrlEncoder.Default, new FakeStringLocalizer(),
            Options.Create(identityStuff.NuagesOptions), new Mock<IMessageService>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await service.GetMFAUrlAsync(MockHelpers.BadId);
        });
    }

    [Fact]
    public async Task ShouldResetRecoveryCodesWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        var service = new MFAService(identityStuff.UserManager, UrlEncoder.Default, new FakeStringLocalizer(),
            Options.Create(identityStuff.NuagesOptions), new Mock<IMessageService>().Object);

        var res = await service.ResetRecoveryCodesAsync(user.Id);
        
        Assert.True(res.Success);
        Assert.Equal(10, res.Codes.Count);

        var res2 = await service.GetRecoveryCodes(user.Id);
        Assert.Equal(10, res2.Count);
    }
    
    [Fact]
    public async Task ShouldResetRecoveryCodesWithErrorNotFOund()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        var service = new MFAService(identityStuff.UserManager, UrlEncoder.Default, new FakeStringLocalizer(),
            Options.Create(identityStuff.NuagesOptions), new Mock<IMessageService>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await service.ResetRecoveryCodesAsync(MockHelpers.BadId);
        });
        
        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await service.GetRecoveryCodes(MockHelpers.BadId);
        });
    }
    
    [Fact]
    public async Task ShouldEnableMfaWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        string? currentKey = null;

        identityStuff.UserAuthenticatorKeyStore.Setup(c =>
                c.GetAuthenticatorKeyAsync(It.IsAny<NuagesApplicationUser<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => currentKey);
        
        identityStuff.UserAuthenticatorKeyStore.Setup(c =>
                c.SetAuthenticatorKeyAsync(It.IsAny<NuagesApplicationUser<string>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback((NuagesApplicationUser<string> _, string key, CancellationToken _) => currentKey = key);
        
        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var service = new MFAService(identityStuff.UserManager, UrlEncoder.Default, new FakeStringLocalizer(),
            Options.Create(identityStuff.NuagesOptions),messageService.Object);

        var res = await service.EnableMFAAsync(user.Id, MockHelpers.ValidToken);
        
        Assert.True(res.Success);
        Assert.True(sendCalled);
    }
    
    [Fact]
    public async Task ShouldEnableMfaWithErrorBadToken()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

       
        
        var service = new MFAService(identityStuff.UserManager, UrlEncoder.Default, new FakeStringLocalizer(),
            Options.Create(identityStuff.NuagesOptions), new Mock<IMessageService>().Object);

        var res = await service.EnableMFAAsync(user.Id, "bad_token");
        
        Assert.False(res.Success);
        Assert.Equal("InvalidVerificationCode", res.Errors.Single());
    }
    
    [Fact]
    public async Task ShouldEnableMfaWithExceptionNotFOund()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var service = new MFAService(identityStuff.UserManager, UrlEncoder.Default, new FakeStringLocalizer(),
            Options.Create(identityStuff.NuagesOptions), new Mock<IMessageService>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await service.EnableMFAAsync(MockHelpers.BadId, MockHelpers.ValidToken);
        });
    }
    
    [Fact]
    public async Task ShouldDisableMfaWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var sendCalled = false;
        
        var messageService = new Mock<IMessageService>();
        messageService.Setup(m => m.SendEmailUsingTemplate(user.Email, It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>?>(), It.IsAny<string?>()))
            .Callback(() => sendCalled = true);
        
        var service = new MFAService(identityStuff.UserManager, UrlEncoder.Default, new FakeStringLocalizer(),
            Options.Create(identityStuff.NuagesOptions),messageService.Object);

        var res = await service.DisableMFAAsync(user.Id);
        
        Assert.True(res.Success);
        Assert.True(sendCalled);
    }
    
    [Fact]
    public async Task ShouldDisableMfaWithExceptionNotFound()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

      
        var service = new MFAService(identityStuff.UserManager, UrlEncoder.Default, new FakeStringLocalizer(),
            Options.Create(identityStuff.NuagesOptions), new Mock<IMessageService>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await service.DisableMFAAsync(MockHelpers.BadId);
        });
    }
}