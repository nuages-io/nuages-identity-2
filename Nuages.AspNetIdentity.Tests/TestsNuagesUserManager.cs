using Moq;
using Nuages.Identity.Services.Tests;
using Xunit;

namespace Nuages.AspNetIdentity.Tests;

public class TestsNuagesUserManager
{
    private readonly NuagesApplicationUser _defaultUser;
    private readonly MockHelpers.MockIdentity _identityStuff;

    public TestsNuagesUserManager()
    {
        _defaultUser = new NuagesApplicationUser
        {
            Email = "test@example.com",
            UserName = "test"
        };
        
        _identityStuff = MockHelpers.MockIdentityStuff();
    }
    
    [Fact]
    public async Task ShoudlCreateUserWithSuccess()
    {
        Assert.True( (await  _identityStuff.UserManager.CreateAsync(_defaultUser)).Succeeded);
    }
    
    
    [Fact]
    public async Task ShoudlFindUserByEmailWithSuccess()
    {
        var res = await  _identityStuff.UserManager.CreateAsync(_defaultUser);

        Assert.True(res.Succeeded);
        
        Assert.NotNull(await _identityStuff.UserManager.FindAsync(_defaultUser.Email));
    }
    
    [Fact]
    public async Task ShoudlFindUserByUsernameWithSuccess()
    {
        var res = await  _identityStuff.UserManager.CreateAsync(_defaultUser);

        Assert.True(res.Succeeded);
        
        Assert.NotNull(await _identityStuff.UserManager.FindAsync(_defaultUser.UserName));
    }
    
    [Fact]
    public async Task ShoudlAddPasswordWithSuccess()
    {
        var res = await  _identityStuff.UserManager.CreateAsync(_defaultUser);

        Assert.True(res.Succeeded);
        
        res = await _identityStuff.UserManager.AddPasswordAsync(_defaultUser, MockHelpers.StrongPassword);
        
        Assert.True(res.Succeeded);
    }
    
    [Fact]
    public async Task ShoudlAddPasswordWithErrorAlreadyHavePassowrd()
    {
        var res = await  _identityStuff.UserManager.CreateAsync(_defaultUser);

        Assert.True(res.Succeeded);
        
        res = await _identityStuff.UserManager.AddPasswordAsync(_defaultUser, MockHelpers.StrongPassword);
        
        Assert.True(res.Succeeded);
        
        res = await _identityStuff.UserManager.AddPasswordAsync(_defaultUser, MockHelpers.StrongPassword);
        
        Assert.False(res.Succeeded);
    }

    
    [Fact]
    public async Task ShoudlChangePasswordWithSuccess()
    {
        var res = await  _identityStuff.UserManager.CreateAsync(_defaultUser);

        Assert.True(res.Succeeded);
        
        res = await _identityStuff.UserManager.AddPasswordAsync(_defaultUser, MockHelpers.StrongPassword);
        
        Assert.True(res.Succeeded);

        res = await _identityStuff.UserManager.ChangePasswordAsync(_defaultUser, MockHelpers.StrongPassword,  MockHelpers.StrongPassword + "New");
        
        Assert.True(res.Succeeded);
    }
    
    [Fact]
    public async Task ShoudlChangePasswordWithErrorNotStrongEnough()
    {
        var res = await  _identityStuff.UserManager.CreateAsync(_defaultUser);

        Assert.True(res.Succeeded);
        
        res = await _identityStuff.UserManager.AddPasswordAsync(_defaultUser, MockHelpers.StrongPassword);
        
        Assert.True(res.Succeeded);
        
        res = await _identityStuff.UserManager.ChangePasswordAsync(_defaultUser, MockHelpers.StrongPassword,  MockHelpers.WeakPassword);
        
        Assert.False(res.Succeeded);
    }
    
}