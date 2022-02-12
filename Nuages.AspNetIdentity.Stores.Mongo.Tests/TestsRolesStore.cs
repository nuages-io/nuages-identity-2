using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Nuages.AspNetIdentity.Core;
using Xunit;

namespace Nuages.AspNetIdentity.Stores.Mongo.Tests;

[Collection("Mongo")]
public class TestsRolesStore
{
    private readonly MongoNoSqlRoleStore<NuagesApplicationRole<string>, string> _noSqlRoleStore;

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

        var options = new MongoIdentityOptions
        {
            ConnectionString = config["ConnectionString"],
            Database = config["Database"]
        };

        var client = new MongoClient(options.ConnectionString);
        client.DropDatabase(options.Database);

        _noSqlRoleStore = new MongoNoSqlRoleStore<NuagesApplicationRole<string>, string>(Options.Create(options));
    }

    [Fact]
    public async Task ShouldCreateWithSuccess()
    {
        var role = new NuagesApplicationRole<string>
        {
            Name = "Role",
            NormalizedName = "ROLE"
        };

        var res = await _noSqlRoleStore.CreateAsync(role, CancellationToken.None);

        Assert.True(res.Succeeded);

        await _noSqlRoleStore.UpdateAsync(role, CancellationToken.None);

        var name = await _noSqlRoleStore.GetRoleNameAsync(role, CancellationToken.None);
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

        var res = await _noSqlRoleStore.CreateAsync(role, CancellationToken.None);

        Assert.True(res.Succeeded);

        var id = await _noSqlRoleStore.GetRoleIdAsync(role, CancellationToken.None);
        Assert.NotNull(id);
        Assert.NotNull(await _noSqlRoleStore.FindByIdAsync(id!, CancellationToken.None));

        await _noSqlRoleStore.DeleteAsync(role, CancellationToken.None);

        Assert.Null(await _noSqlRoleStore.FindByIdAsync(id!, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldFindByNameWithSuccess()
    {
        var role = new NuagesApplicationRole<string>
        {
            Name = "Role",
            NormalizedName = "ROLE"
        };

        var res = await _noSqlRoleStore.CreateAsync(role, CancellationToken.None);

        Assert.True(res.Succeeded);


        var name = await _noSqlRoleStore.GetNormalizedRoleNameAsync(role, CancellationToken.None);

        Assert.NotNull(await _noSqlRoleStore.FindByNameAsync(name, CancellationToken.None));
        const string newName = "NewName";

        await _noSqlRoleStore.SetRoleNameAsync(role, newName, CancellationToken.None);
        await _noSqlRoleStore.SetNormalizedRoleNameAsync(role, newName.ToUpper(), CancellationToken.None);

        Assert.NotNull(await _noSqlRoleStore.FindByNameAsync(newName, CancellationToken.None));
        Assert.NotNull(await _noSqlRoleStore.FindByNameAsync(newName.ToUpper(), CancellationToken.None));
    }


    [Fact]
    public async Task ShouldFAddCLaimsWithSuccess()
    {
        var role = new NuagesApplicationRole<string>
        {
            Name = "Role",
            NormalizedName = "ROLE"
        };

        var res = await _noSqlRoleStore.CreateAsync(role, CancellationToken.None);

        Assert.True(res.Succeeded);

        var claim = new Claim("name", "value");

        await _noSqlRoleStore.AddClaimAsync(role, claim, CancellationToken.None);

        var claims = await _noSqlRoleStore.GetClaimsAsync(role, CancellationToken.None);

        Assert.Single(claims);

        await _noSqlRoleStore.RemoveClaimAsync(role, claim, CancellationToken.None);

        claims = await _noSqlRoleStore.GetClaimsAsync(role, CancellationToken.None);

        Assert.Empty(claims);
    }
}