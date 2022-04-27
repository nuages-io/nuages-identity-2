using Microsoft.AspNetCore.Identity;
using Moq;
using Nuages.Identity.Services.Manage;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsProfileService
{
    [Fact]
    public async Task ShouldSaveProfileWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var profileService = new ProfileService(identityStuff.UserManager, new FakeStringLocalizer(), new Mock<IIdentityEventBus>().Object);

        const string newLastName = "LastName";
        const string newFirstName = "FirstName";

        var res = await profileService.SaveProfile(user.Id, new SaveProfileModel
        {
            FirstName = newFirstName,
            LastName = newLastName
        });

        Assert.True(res.Success);
        Assert.Equal(newLastName, user.LastName);
        Assert.Equal(newFirstName, user.FirstName);
    }

    [Fact]
    public async Task ShouldSaveProfileWithFailure()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var profileService = new ProfileService(identityStuff.UserManager, new FakeStringLocalizer(), new Mock<IIdentityEventBus>().Object);

        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await profileService.SaveProfile(MockHelpers.BadId, new SaveProfileModel
            {
                FirstName = "FirstName",
                LastName = "LastName"
            });
        });
    }

    [Fact]
    public async Task ShouldSaveProfileWithFailureAndErrors()
    {
        var user = MockHelpers.CreateDefaultUser();

        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.UserStore.Setup(u => u.UpdateAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(() =>
            IdentityResult.Failed(new IdentityError { Code = "error", Description = "error" }));


        var profileService = new ProfileService(identityStuff.UserManager, new FakeStringLocalizer(), new Mock<IIdentityEventBus>().Object);

        var res = await profileService.SaveProfile(user.Id, new SaveProfileModel
        {
            FirstName = "FirstName",
            LastName = "LastName"
        });

        Assert.False(res.Success);
        Assert.Equal("identity.error", res.Errors.Single());
    }
}