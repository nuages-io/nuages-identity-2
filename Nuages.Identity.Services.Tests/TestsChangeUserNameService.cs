using Microsoft.AspNetCore.Identity;
using Moq;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.Services.Manage;
using Nuages.Web.Exceptions;
using Nuages.Web.Utilities;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsChangeUserNameService
{
    [Fact]
    public async Task ShouldChangeUSerNameWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
            
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var changeUserNameSrvice =
            new ChangeUserNameService(identityStuff.UserManager, new FakeStringLocalizer(), new EmailValidator());

        var res = await changeUserNameSrvice.ChangeUserNameAsync(user.Id, "new_user_name");
        
        Assert.True(res.Success);
    }
    
    [Fact]
    public async Task ShouldChangeUserNameThrowsException()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var changeUserNameSrvice =
            new ChangeUserNameService(identityStuff.UserManager, new FakeStringLocalizer(), new EmailValidator());

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await changeUserNameSrvice.ChangeUserNameAsync(MockHelpers.BadId, "new_user_name");

        });
    }
    
    [Fact]
    public async Task ShouldChangeUSerNameWithErrors()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.UserStore.Setup(u => u.UpdateAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync( () => IdentityResult.Failed(new IdentityError { Code = "error", Description = "error"}) );

        var changeUserNameSrvice =
            new ChangeUserNameService(identityStuff.UserManager, new FakeStringLocalizer(), new EmailValidator());

        var res = await changeUserNameSrvice.ChangeUserNameAsync(user.Id, "new_user_name");
        
        Assert.False(res.Success);
        Assert.Equal("identity.error", res.Errors.Single());
    }
    
    [Fact]
    public async Task ShouldChangeUSerNameWithErrorsInvalidIsEmail()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.UserStore.Setup(u => u.UpdateAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync( () => IdentityResult.Failed(new IdentityError { Code = "error", Description = "error"}) );

        var changeUserNameSrvice =
            new ChangeUserNameService(identityStuff.UserManager, new FakeStringLocalizer(), new EmailValidator());

        var res = await changeUserNameSrvice.ChangeUserNameAsync(user.Id, "TEST2@NUAGES.ORG");
        
        Assert.False(res.Success);
        Assert.Equal("changeUsername:mustNotBeAnEmail", res.Errors.Single());
    }
    
    [Fact]
    public async Task ShouldChangeUSerNameWithErrorsUsernameNotChanged()
    {
        var user = MockHelpers.CreateDefaultUser();
        user.UserName = "USERNAME";
        user.NormalizedUserName = "USERNAME";
        
       
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.UserStore.Setup(u => u.FindByNameAsync(user.UserName, It.IsAny<CancellationToken>())).ReturnsAsync( () => user );

        var changeUserNameSrvice =
            new ChangeUserNameService(identityStuff.UserManager, new FakeStringLocalizer(), new EmailValidator());

        var res = await changeUserNameSrvice.ChangeUserNameAsync(user.Id, user.UserName);
        
        Assert.False(res.Success);
        Assert.Equal("changeUsername:isNotChanged", res.Errors.Single());
    }
    
    [Fact]
    public async Task ShouldChangeUSerNameWithErrorsUsernameAlreadyUsed()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.UserStore.Setup(u => u.FindByNameAsync("EXISTING", It.IsAny<CancellationToken>())).ReturnsAsync( () => new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString()
        });

        var changeUserNameSrvice =
            new ChangeUserNameService(identityStuff.UserManager, new FakeStringLocalizer(), new EmailValidator());

        var res = await changeUserNameSrvice.ChangeUserNameAsync(user.Id, "EXISTING");
        
        Assert.False(res.Success);
        Assert.Equal("changeUsername:nameAlreadyUsed", res.Errors.Single());
    }
}