using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Nuages.AspNetIdentity.Stores.InMemory;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once UnusedType.Global
public class InMemoryUserStore<TUser, TRole, TKey> : UserStoreBase<TUser, TRole, TKey, 
                        IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityUserRole<TKey>>,
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
    private IQueryable<IdentityUserClaim<TKey>> UsersClaims => _userClaims.AsQueryable();
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


    public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        foreach (var claim in claims)
        {
            _userClaims.Add(new IdentityUserClaim<TKey>
            {
                UserId = user.Id,
                ClaimType = claim.Type,
                ClaimValue = claim.Value
            });
        }

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
    
    protected override async Task<TRole?> FindRoleByNameAsync(string normalizedName,
        CancellationToken cancellationToken)
    {
        return await RoleStore.FindByNameAsync(normalizedName, cancellationToken);
    }
    
    protected override Task AddUserTokenAsync(IdentityUserToken<TKey> token)
    {
        _userTokens.Add(token);
        
        return Task.CompletedTask;
    }

    protected override Task UpdateUserTokenAsync(IdentityUserToken<TKey> token)
    {
        return Task.CompletedTask;
    }

    protected override Task RemoveTokenAsync(IdentityUserToken<TKey> token)
    {
        _userTokens.Remove(token);

        return Task.CompletedTask;
    }
    
    public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
    {
        var list = UsersClaims.Where(c => c.UserId.Equals(user.Id)).Select(c =>
            new Claim(c.ClaimType, c.ClaimValue)
        ).ToList();
        
        return Task.FromResult((IList<Claim>) list);
    }

    public Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        var ids = UsersClaims.Where(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value).Select(c => c.UserId);
        
        return Task.FromResult<IList<TUser>>(Users.Where(i => ids.Contains(i.Id)).ToList());
    }
    
    public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        var matchedClaims = UsersClaims.Where(c =>
            c.ClaimType == claim.Type && c.ClaimValue == claim.Value && user.Id.Equals(user.Id));

        foreach (var matchedClaim in matchedClaims)
        {
            matchedClaim.ClaimValue = newClaim.Value;
            matchedClaim.ClaimType = newClaim.Type;
        }

        return Task.CompletedTask;
    }
    
    
    public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        foreach (var claim in claims)
        {
            var entity =
                UsersClaims.FirstOrDefault(
                    uc => uc.UserId.Equals(user.Id) && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value);
            if (entity != null)
            {
                _userClaims.Remove(entity);
            }
        }

        return Task.CompletedTask;
    }

}