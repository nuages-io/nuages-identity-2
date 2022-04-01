using Nuages.Identity.Services.AspNetIdentity;
using Xunit;

namespace Nuages.Identity.Services.Tests.AspNetIdentity;

[Collection("InMemoryTests")]
public class TestsNuagesUserManager
{
    private readonly NuagesApplicationUser<string> _defaultUser;
    private readonly MockHelpersAspNetIdentity.MockIdentity _identityStuff;

    public TestsNuagesUserManager()
    {
        _defaultUser = new NuagesApplicationUser<string>
        {
            Email = "test@example.com",
            UserName = "test"
        };

        _identityStuff = MockHelpersAspNetIdentity.MockIdentityStuff();

        _identityStuff.DataContext.Database.EnsureDeleted();
    }

    [Fact]
    public async Task ShoudlCreateUserWithSuccess()
    {
        Assert.True((await _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
    }


    [Fact]
    public async Task ShoudlFindUserByEmailWithSuccess()
    {
        var res = await _identityStuff.UserManager.CreateAsync(_defaultUser);

        Assert.True(res.Succeeded);

        Assert.NotNull(await _identityStuff.UserManager.FindAsync(_defaultUser.Email));
    }

    [Fact]
    public async Task ShoudlFindUserByUsernameWithSuccess()
    {
        var res = await _identityStuff.UserManager.CreateAsync(_defaultUser);

        Assert.True(res.Succeeded);

        Assert.NotNull(await _identityStuff.UserManager.FindAsync(_defaultUser.UserName));
    }

    [Fact]
    public async Task ShoudlAddPasswordWithSuccess()
    {
        var res = await _identityStuff.UserManager.CreateAsync(_defaultUser);

        Assert.True(res.Succeeded);

        res = await _identityStuff.UserManager.AddPasswordAsync(_defaultUser, MockHelpersAspNetIdentity.StrongPassword);

        Assert.True(res.Succeeded);
    }

    [Fact]
    public async Task ShoudlAddPasswordWithErrorAlreadyHavePassowrd()
    {
        var res = await _identityStuff.UserManager.CreateAsync(_defaultUser);

        Assert.True(res.Succeeded);

        res = await _identityStuff.UserManager.AddPasswordAsync(_defaultUser, MockHelpersAspNetIdentity.StrongPassword);

        Assert.True(res.Succeeded);

        res = await _identityStuff.UserManager.AddPasswordAsync(_defaultUser, MockHelpersAspNetIdentity.StrongPassword);

        Assert.False(res.Succeeded);
    }


    [Fact]
    public async Task ShoudlChangePasswordWithSuccess()
    {
        var res = await _identityStuff.UserManager.CreateAsync(_defaultUser);

        Assert.True(res.Succeeded);

        res = await _identityStuff.UserManager.AddPasswordAsync(_defaultUser, MockHelpersAspNetIdentity.StrongPassword);

        Assert.True(res.Succeeded);

        res = await _identityStuff.UserManager.ChangePasswordAsync(_defaultUser, MockHelpersAspNetIdentity.StrongPassword,
            MockHelpersAspNetIdentity.StrongPassword + "New");

        Assert.True(res.Succeeded);
    }

    [Fact]
    public async Task ShoudlChangePasswordWithErrorNotStrongEnough()
    {
        var res = await _identityStuff.UserManager.CreateAsync(_defaultUser);

        Assert.True(res.Succeeded);

        res = await _identityStuff.UserManager.AddPasswordAsync(_defaultUser, MockHelpersAspNetIdentity.StrongPassword);

        Assert.True(res.Succeeded);

        res = await _identityStuff.UserManager.ChangePasswordAsync(_defaultUser, MockHelpersAspNetIdentity.StrongPassword,
            MockHelpersAspNetIdentity.WeakPassword);

        Assert.False(res.Succeeded);
    }
}