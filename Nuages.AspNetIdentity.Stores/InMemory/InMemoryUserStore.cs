using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Nuages.AspNetIdentity.Stores.InMemory;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once UnusedType.Global
public class InMemoryUserStore<TUser, TRole, TKey> : UserStoreBase<TUser, TRole, TKey>,
    IUserClaimStore<TUser>,
    IUserLoginStore<TUser>,
    IUserRoleStore<TUser>,
    IUserPasswordStore<TUser>,
    IUserEmailStore<TUser>,
    IUserPhoneNumberStore<TUser>,
    IQueryableUserStore<TUser>,
    IUserTwoFactorStore<TUser>,
    IUserLockoutStore<TUser>,
    IUserAuthenticatorKeyStore<TUser>,
    IUserAuthenticationTokenStore<TUser>,
    IUserTwoFactorRecoveryCodeStore<TUser>,
    IProtectedUserStore<TUser>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
   [ExcludeFromCodeCoverage]
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public InMemoryUserStore()
    {
        RoleStore = new InMemoryRoleStore<TRole, TKey>();
    }
    
    private readonly List<TUser> _users = new ();
    
    private readonly List<IdentityUserClaim<TKey>> _userClaims = new();
    private readonly List<IdentityUserLogin<TKey>> _userLogins = new();
    private readonly List<IdentityUserToken<TKey>> _userTokens = new();
    private readonly List<IdentityUserRole<TKey>> _userRoles = new();
    // ReSharper disable once CollectionNeverUpdated.Local
   
    public readonly InMemoryRoleStore<TRole, TKey> RoleStore;

    public override IQueryable<TUser> Users => _users.AsQueryable();
    protected override IQueryable<TRole> Roles => RoleStore.Roles;
    protected override IQueryable<IdentityUserClaim<TKey>> UsersClaims => _userClaims.AsQueryable();
    protected override IQueryable<IdentityUserLogin<TKey>> UsersLogins => _userLogins.AsQueryable();
    protected override IQueryable<IdentityUserToken<TKey>> UsersTokens => _userTokens.AsQueryable();
    protected override IQueryable<IdentityUserRole<TKey>> UsersRoles =>_userRoles.AsQueryable();
    
    
    public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
    {
        user.Id = StringToKey(Guid.NewGuid().ToString());
        _users.Add(user);

        var username = await GetUserNameAsync(user, cancellationToken);

        if (string.IsNullOrEmpty(username))
        {
            await SetUserNameAsync(user, user.Email, cancellationToken);
        }

        await SetNormalizedUserNameAsync(user, user.UserName.ToUpper(), cancellationToken);
        await SetNormalizedEmailAsync(user, user.Email.ToUpper(), cancellationToken);
        
        return IdentityResult.Success;
    }

    public override Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
    {
        _users.Remove(user);
        
        return Task.FromResult(IdentityResult.Success);
    }

    protected override Task AddClaim(IdentityUserClaim<TKey> claim)
    {
        _userClaims.Add(claim);
        
        return Task.CompletedTask;
    }

    protected override Task UpdateClaimAsync(IdentityUserClaim<TKey> claim,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        _userLogins.Add(new IdentityUserLogin<TKey>
        {
            UserId = user.Id,
            ProviderKey = login.ProviderKey,
            LoginProvider = login.LoginProvider,
            ProviderDisplayName = login.ProviderDisplayName
        });

        return Task.CompletedTask;
    }

    protected override Task RemoveUserLoginAsync(IdentityUserLogin<TKey> login)
    {
        _userLogins.Remove(login);

        return Task.CompletedTask;
    }

    protected override Task RemoveUserClaimAsync(IdentityUserClaim<TKey> claim)
    {
        _userClaims.Remove(claim);

        return Task.CompletedTask;
    }

    protected override Task RemoveUserRoleAsync(IdentityUserRole<TKey> role)
    {
        _userRoles.Remove(role);

        return Task.CompletedTask;
    }

    protected override Task AddUserRoleAsync(IdentityUserRole<TKey> role)
    {
        _userRoles.Add(role);

        return Task.CompletedTask;
    }
    
    protected override async Task<TRole> FindRoleByNameAsync(string nomalizedName, CancellationToken cancellationToken)
    {
        return await RoleStore.FindByNameAsync(nomalizedName, cancellationToken);
    }
    
    protected override Task AddUserTokenAsync(IdentityUserToken<TKey> token)
    {
        _userTokens.Add(token);
        
        return Task.CompletedTask;
    }

    protected override Task RemoveTokenAsync(IdentityUserToken<TKey> token)
    {
        _userTokens.Remove(token);

        return Task.CompletedTask;
    }

}