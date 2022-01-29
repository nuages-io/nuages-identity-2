using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Manage;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsChangeEmailService
{
    [Fact]
    public async Task ChangeEmailWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var changeEmailService = new ChangeEmailService(identityStuff.UserManager, new FakeStringLocalizer());

        const string newEmail = "newEmail@nuages.org";
        
        var token = await identityStuff.UserManager.GenerateChangeEmailTokenAsync(user, newEmail);
        
        var res = await changeEmailService.ChangeEmailAsync(user.Id, newEmail, token);
        
        Assert.True(res.Success);
    }
    
    [Fact]
    public async Task ChangeEmailWithSuccessWithoutToken()
    {
        var user = MockHelpers.CreateDefaultUser();
        user.UserName = user.Email;
        user.NormalizedUserName = user.NormalizedEmail;
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var changeEmailService = new ChangeEmailService(identityStuff.UserManager, new FakeStringLocalizer());

        const string newEmail = "newEmail@nuages.org";
        
        var res = await changeEmailService.ChangeEmailAsync(user.Id, newEmail, null);
        
        Assert.True(res.Success);
    }
    
    [Fact]
    public async Task ChangeEmailWithErrorNotChanged()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
            
        var changeEmailService = new ChangeEmailService(identityStuff.UserManager, new FakeStringLocalizer());
        
        var res = await changeEmailService.ChangeEmailAsync(user.Id, user.Email, null);
        
        Assert.False(res.Success);
        Assert.Equal("changeEmail:isNotChanged", res.Errors.Single());
    }
    
    [Fact]
    public async Task ChangeEmailWithErrorAlreadyExists()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var newEmail = "newEmail@nuages.org".ToUpper();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.UserEmaiLStore.Setup(u => 
            u.FindByEmailAsync(newEmail, It.IsAny<CancellationToken>())).ReturnsAsync( () => new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString()
        });
        
        var changeEmailService = new ChangeEmailService(identityStuff.UserManager, new FakeStringLocalizer());
        
        var res = await changeEmailService.ChangeEmailAsync(user.Id, newEmail, null);
        
        Assert.False(res.Success);
        Assert.Equal("changeEmail:emailAlreadyUsed", res.Errors.Single());
    }
    
    [Fact]
    public async Task ChangeEmailWithErrorNotFoundThrowException()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
    
        var changeEmailService = new ChangeEmailService(identityStuff.UserManager, new FakeStringLocalizer());
        
        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await changeEmailService.ChangeEmailAsync("bad_id", "newEmail@nuages.org".ToUpper(), null);
        });
        
    }

}