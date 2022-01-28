using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Manage;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsProfileService
{
    [Fact]
    public async Task ShouldSaveProfileWithSuccess()
    {
        var email = "TEST@NUAGES.ORG";
        
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email,
            UserName = email,
            NormalizedUserName = email,
            EmailConfirmed = true
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);

        
        var profileService = new ProfileService(identityStuff.UserManager, new FakeStringLocalizer());

        var res = await profileService.SaveProfile(user.Id, new SaveProfileModel
        {
            FirstName = "FirstName",
            LastName = "LastName"
        });
        
        Assert.True(res.Success);

    }
    
    [Fact]
    public async Task ShouldSaveProfileWithFailure()
    {
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString()
        };
        
        var identityStuff = MockHelpers.MockIdentityStuff(user, new NuagesIdentityOptions());

        var profileService = new ProfileService(identityStuff.UserManager, new FakeStringLocalizer());

        
        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await profileService.SaveProfile("bad_id", new SaveProfileModel
            {
                FirstName = "FirstName",
                LastName = "LastName"
            });
            
        });
        
    }
    
    [Fact]
    public async Task ShouldSaveProfileWithFailureAndErrors()
    {
        var email = "TEST@NUAGES.ORG";
        
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email,
            UserName = email,
            NormalizedUserName = email,
            EmailConfirmed = true
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);
        identityStuff.UserStore.Setup(u => u.UpdateAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync( () => IdentityResult.Failed(new []{ new IdentityError { Code = "error", Description = "error"}}) );

        
        var profileService = new ProfileService(identityStuff.UserManager, new FakeStringLocalizer());

        var res = await profileService.SaveProfile(user.Id, new SaveProfileModel
        {
            FirstName = "FirstName",
            LastName = "LastName"
        });
        
        Assert.False(res.Success);
        Assert.Equal("identity.error", res.Errors.First());

    }
}