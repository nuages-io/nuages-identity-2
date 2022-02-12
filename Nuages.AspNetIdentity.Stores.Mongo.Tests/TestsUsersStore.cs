using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Nuages.AspNetIdentity.Core;
using Xunit;

namespace Nuages.AspNetIdentity.Stores.Mongo.Tests;

[Collection("Mongo")]
public class TestsUsersStore
{
    private readonly MongoNoSqlRoleStore<NuagesApplicationRole<string>, string> _noSqlRoleStore;

    private readonly MongoNoSqlUserStore<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>
        _noSqlUserStore;

    public TestsUsersStore()
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
        _noSqlUserStore =
            new MongoNoSqlUserStore<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>(
                Options.Create(options));
    }

    private async Task<NuagesApplicationUser<string>> CreateDefaultUser()
    {
        const string email = "user@example.com";

        var user = new NuagesApplicationUser<string>
        {
            Email = email,
            NormalizedEmail = email.ToUpper(),
            UserName = email,
            NormalizedUserName = email.ToUpper()
        };

        var res = await _noSqlUserStore.CreateAsync(user, CancellationToken.None);

        Assert.True(res.Succeeded);

        return user;
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private async Task<NuagesApplicationUser<string>> CreateDefaultUser2()
    {
        var user = new NuagesApplicationUser<string>
        {
            Email = "user2@example.com"
        };

        var res = await _noSqlUserStore.CreateAsync(user, CancellationToken.None);

        Assert.True(res.Succeeded);


        return user;
    }

    [Fact]
    public async Task ShouldCreateWithSuccess()
    {
        var user = await CreateDefaultUser();

        Assert.NotNull(await _noSqlUserStore.FindByEmailAsync(user.Email, CancellationToken.None));
        Assert.NotNull(await _noSqlUserStore.FindByIdAsync(user.Id, CancellationToken.None));

        await _noSqlUserStore.SetSecurityStampAsync(user, "stamp", CancellationToken.None);
        Assert.Equal("stamp", await _noSqlUserStore.GetSecurityStampAsync(user, CancellationToken.None));

        user = await _noSqlUserStore.FindByNameAsync(user.UserName, CancellationToken.None);
        Assert.NotNull(user);
        var id = await _noSqlUserStore.GetUserIdAsync(user, CancellationToken.None);
        Assert.Equal(user.Id, id);

        await _noSqlUserStore.DeleteAsync(user, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.Null(user);
    }

    [Fact]
    public async Task ShouldIncrementAccessFailedCount()
    {
        var user = await CreateDefaultUser();

        Assert.Equal(0, await _noSqlUserStore.GetAccessFailedCountAsync(user, CancellationToken.None));

        Assert.Equal(1, await _noSqlUserStore.IncrementAccessFailedCountAsync(user, CancellationToken.None));

        user = await ReloadAsync(user);

        Assert.Equal(1, await _noSqlUserStore.GetAccessFailedCountAsync(user, CancellationToken.None));

        await _noSqlUserStore.ResetAccessFailedCountAsync(user, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.Equal(0, await _noSqlUserStore.GetAccessFailedCountAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldSetEmailWithSuccess()
    {
        var user = await CreateDefaultUser();

        const string newUserName = "test2@example.com";

        await _noSqlUserStore.SetEmailAsync(user, newUserName, CancellationToken.None);
        await _noSqlUserStore.SetNormalizedEmailAsync(user, newUserName.ToUpper(), CancellationToken.None);

        await _noSqlUserStore.SetEmailConfirmedAsync(user, true, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.Equal(newUserName, await _noSqlUserStore.GetEmailAsync(user, CancellationToken.None));

        Assert.True(await _noSqlUserStore.GetEmailConfirmedAsync(user, CancellationToken.None));

        Assert.Equal(user.Email.ToUpper(), await _noSqlUserStore.GetNormalizedEmailAsync(user, CancellationToken.None));
        Assert.Equal(user.UserName.ToUpper(),
            await _noSqlUserStore.GetNormalizedUserNameAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldSetPhoneNumberWithSuccess()
    {
        var user = await CreateDefaultUser();

        const string newPhoneNUmber = "9999999999";

        await _noSqlUserStore.SetPhoneNumberAsync(user, newPhoneNUmber, CancellationToken.None);
        await _noSqlUserStore.SetPhoneNumberConfirmedAsync(user, true, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.Equal(newPhoneNUmber, await _noSqlUserStore.GetPhoneNumberAsync(user, CancellationToken.None));
        Assert.True(await _noSqlUserStore.GetPhoneNumberConfirmedAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldSetTwoFactorEnabledWithSuccess()
    {
        var user = await CreateDefaultUser();

        await _noSqlUserStore.SetTwoFactorEnabledAsync(user, true, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.True(await _noSqlUserStore.GetTwoFactorEnabledAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldSetLockoutEnabledWithSuccess()
    {
        var user = await CreateDefaultUser();

        await _noSqlUserStore.SetLockoutEnabledAsync(user, true, CancellationToken.None);
        await _noSqlUserStore.SetLockoutEndDateAsync(user, DateTimeOffset.Now, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.True(await _noSqlUserStore.GetLockoutEnabledAsync(user, CancellationToken.None));
        Assert.NotNull(await _noSqlUserStore.GetLockoutEndDateAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldSetPasswordHashWithSuccess()
    {
        var user = await CreateDefaultUser();

        await _noSqlUserStore.SetPasswordHashAsync(user, "hash", CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.True(await _noSqlUserStore.HasPasswordAsync(user, CancellationToken.None));
        Assert.NotNull(await _noSqlUserStore.GetPasswordHashAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldReplaceCodesWithSuccess()
    {
        var user = await CreateDefaultUser();

        Assert.Equal(0, await _noSqlUserStore.CountCodesAsync(user, CancellationToken.None));

        var codes = new[] { "a", "b", "c" };

        await _noSqlUserStore.ReplaceCodesAsync(user, codes, CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.Equal(3, await _noSqlUserStore.CountCodesAsync(user, CancellationToken.None));

        Assert.False(await _noSqlUserStore.RedeemCodeAsync(user, "bad_code", CancellationToken.None));
        Assert.True(await _noSqlUserStore.RedeemCodeAsync(user, codes.First(), CancellationToken.None));

        user = await ReloadAsync(user);

        Assert.Equal(2, await _noSqlUserStore.CountCodesAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldSetAuthenticatorWithSuccess()
    {
        var user = await CreateDefaultUser();

        await _noSqlUserStore.SetAuthenticatorKeyAsync(user, "key", CancellationToken.None);

        user = await _noSqlUserStore.FindByIdAsync(user.Id, CancellationToken.None);

        Assert.Equal("key", await _noSqlUserStore.GetAuthenticatorKeyAsync(user, CancellationToken.None));

        await _noSqlUserStore.RemoveTokenAsync(user,
            AuthenticatorInfo.AuthenticatorStoreLoginProvider,
            AuthenticatorInfo.AuthenticatorKeyTokenName,
            CancellationToken.None);

        user = await ReloadAsync(user);

        Assert.Null(await _noSqlUserStore.GetAuthenticatorKeyAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldAddCLaimWithSuccess()
    {
        var user = await CreateDefaultUser();
        await CreateDefaultUser2();

        var claim = new Claim("claim", "value");

        await _noSqlUserStore.AddClaimsAsync(user, new[] { claim }, CancellationToken.None);

        user = await ReloadAsync(user);

        var claims = await _noSqlUserStore.GetClaimsAsync(user, CancellationToken.None);
        Assert.Equal(1, claims.Count);

        var users = await _noSqlUserStore.GetUsersForClaimAsync(claim, CancellationToken.None);

        Assert.Equal(1, users.Count);

        var claim2 = new Claim("claim2", "value2");

        await _noSqlUserStore.ReplaceClaimAsync(user, claim, claim2, CancellationToken.None);

        users = await _noSqlUserStore.GetUsersForClaimAsync(claim, CancellationToken.None);

        Assert.Equal(0, users.Count);

        users = await _noSqlUserStore.GetUsersForClaimAsync(claim2, CancellationToken.None);

        Assert.Equal(1, users.Count);

        await _noSqlUserStore.RemoveClaimsAsync(user, new[] { claim2 }, CancellationToken.None);

        user = await ReloadAsync(user);

        claims = await _noSqlUserStore.GetClaimsAsync(user, CancellationToken.None);
        Assert.Equal(0, claims.Count);
    }

    private async Task<NuagesApplicationUser<string>> ReloadAsync(IdentityUser<string> user)
    {
        return await _noSqlUserStore.FindByIdAsync(user.Id, CancellationToken.None);
    }

    [Fact]
    public async Task ShouldAddToRoleWithSuccess()
    {
        const string roleName = "Role";

        var user = await CreateDefaultUser();
        await CreateDefaultUser2();

        await _noSqlRoleStore.CreateAsync(new NuagesApplicationRole<string>
        {
            Name = roleName
        }, CancellationToken.None);

        await _noSqlUserStore.AddToRoleAsync(user, roleName, CancellationToken.None);

        user = await ReloadAsync(user);

        var roles = await _noSqlUserStore.GetRolesAsync(user, CancellationToken.None);

        Assert.Equal(1, roles.Count);

        Assert.True(await _noSqlUserStore.IsInRoleAsync(user, roleName, CancellationToken.None));
        Assert.False(await _noSqlUserStore.IsInRoleAsync(user, "bad_role", CancellationToken.None));

        var users = await _noSqlUserStore.GetUsersInRoleAsync(roleName.ToUpper(), CancellationToken.None);

        Assert.Equal(1, users.Count);

        Assert.Equal(0, (await _noSqlUserStore.GetUsersInRoleAsync("bad_role", CancellationToken.None)).Count);

        await _noSqlUserStore.RemoveFromRoleAsync(user, roleName, CancellationToken.None);

        roles = await _noSqlUserStore.GetRolesAsync(user, CancellationToken.None);

        Assert.Equal(0, roles.Count);
    }

    [Fact]
    public async Task ShouldAddLoginWithSuccess()
    {
        var user = await CreateDefaultUser();
        await CreateDefaultUser2();

        await _noSqlUserStore.AddLoginAsync(user, new UserLoginInfo("provider", "key", "display"),
            CancellationToken.None);

        user = await ReloadAsync(user);

        var logins = await _noSqlUserStore.GetLoginsAsync(user, CancellationToken.None);
        Assert.Equal(1, logins.Count);

        Assert.NotNull(await _noSqlUserStore.FindByLoginAsync("provider", "key", CancellationToken.None));
        Assert.Null(await _noSqlUserStore.FindByLoginAsync("provider", "bad_key", CancellationToken.None));

        await _noSqlUserStore.RemoveLoginAsync(user, "provider", "key", CancellationToken.None);

        user = await ReloadAsync(user);

        logins = await _noSqlUserStore.GetLoginsAsync(user, CancellationToken.None);
        Assert.Equal(0, logins.Count);
    }
}