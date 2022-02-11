using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Nuages.AspNetIdentity.Core;
using Nuages.AspNetIdentity.Stores.InMemory;
using Xunit;

namespace Nuages.AspNetIdentity.Stores.Tests;

public class TestsInMemoryRoleStore
{
    private readonly InMemoryRoleStore<NuagesApplicationRole<string>, string> _roleStore;

    public TestsInMemoryRoleStore()
    {
        var inmemoryRoleStorage = new InMemoryStorage<NuagesApplicationRole<string>, string>();
        _roleStore = new InMemoryRoleStore<NuagesApplicationRole<string>, string>(inmemoryRoleStorage);
    }
    
    [Fact]
    public async Task ShouldCreateWithSuccess()
    {
        var role = new NuagesApplicationRole<string>
        {
            Name = "Role",
            NormalizedName = "ROLE"
        };

        var res = await _roleStore.CreateAsync(role, CancellationToken.None);
        
        Assert.True(res.Succeeded);

        await _roleStore.UpdateAsync(role, CancellationToken.None);
        
        var name = await _roleStore.GetRoleNameAsync(role, CancellationToken.None);
        Assert.Equal(role.Name, name);
    }
    
    [Fact]
    public async Task ShouldDeleteWithSuccess()
    {
        var role = new NuagesApplicationRole<string>
        {
            Name = "Role",
            NormalizedName = "ROLE"
        };

        var res = await _roleStore.CreateAsync(role, CancellationToken.None);
        
        Assert.True(res.Succeeded);

        var id = await _roleStore.GetRoleIdAsync(role, CancellationToken.None);
        Assert.NotNull(id);
        Assert.NotNull(await _roleStore.FindByIdAsync(id!, CancellationToken.None));

        await _roleStore.DeleteAsync(role, CancellationToken.None);
        
        Assert.Null(await _roleStore.FindByIdAsync(id!, CancellationToken.None));
    }
    
    [Fact]
    public async Task ShouldFindByNameWithSuccess()
    {
        var role = new NuagesApplicationRole<string>
        {
            Name = "Role",
            NormalizedName = "ROLE"
        };

        var res = await _roleStore.CreateAsync(role, CancellationToken.None);
        
        Assert.True(res.Succeeded);
        
        
        var name = await _roleStore.GetNormalizedRoleNameAsync(role, CancellationToken.None);
        
        Assert.NotNull(await _roleStore.FindByNameAsync(name, CancellationToken.None));
        const string newName = "NewName";
        
        await _roleStore.SetRoleNameAsync(role, newName, CancellationToken.None);
        await _roleStore.SetNormalizedRoleNameAsync(role, newName.ToUpper(), CancellationToken.None);
        
        Assert.NotNull(await _roleStore.FindByNameAsync(newName, CancellationToken.None));
        Assert.NotNull(await _roleStore.FindByNameAsync(newName.ToUpper(), CancellationToken.None));
    }
    
    
    [Fact]
    public async Task ShouldFAddCLaimsWithSuccess()
    {
        var role = new NuagesApplicationRole<string>
        {
            Name = "Role",
            NormalizedName = "ROLE"
        };

        var res = await _roleStore.CreateAsync(role, CancellationToken.None);
        
        Assert.True(res.Succeeded);

        var claim = new Claim("name", "value");

        await _roleStore.AddClaimAsync(role, claim, CancellationToken.None);

        var claims = await _roleStore.GetClaimsAsync(role, CancellationToken.None);

        Assert.Single(claims);

        await _roleStore.RemoveClaimAsync(role, claim, CancellationToken.None);
        
        claims = await _roleStore.GetClaimsAsync(role, CancellationToken.None);
        
        Assert.Empty(claims);
    }
}