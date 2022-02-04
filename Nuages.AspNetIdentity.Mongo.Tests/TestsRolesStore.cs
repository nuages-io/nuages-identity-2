using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Xunit;

namespace Nuages.AspNetIdentity.Mongo.Tests;

public class TestsRolesStore
{
    private readonly MongoRoleStore<NuagesApplicationRole, string> _roleStore;

    public TestsRolesStore()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", false)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var config = serviceProvider.GetRequiredService<IConfiguration>();
        
        MongoIdentityOptions options = new MongoIdentityOptions
        {
            ConnectionString = config["ConnectionString"],
            Database = config["Database"]
        };
            
        var client = new MongoClient(options.ConnectionString);
        client.DropDatabase(options.Database);
        
        _roleStore = new MongoRoleStore<NuagesApplicationRole, string>(Options.Create(options));
    }
    
    [Fact]
    public async Task ShouldCreateWithSuccess()
    {
        var role = new NuagesApplicationRole
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
        var role = new NuagesApplicationRole
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
        var role = new NuagesApplicationRole
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
        var role = new NuagesApplicationRole
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