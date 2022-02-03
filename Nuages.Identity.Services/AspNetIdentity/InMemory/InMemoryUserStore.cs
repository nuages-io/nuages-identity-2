using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Nuages.Identity.Services.AspNetIdentity.InMemory;

// ReSharper disable once ClassNeverInstantiated.Global
public class InMemoryUserStore<TUser, TRole, TKey> : 
    IUserClaimStore<TUser>,
    IUserLoginStore<TUser>,
    IUserRoleStore<TUser>,
    IUserPasswordStore<TUser>,
    IUserSecurityStampStore<TUser>,
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
    public void Dispose()
    {
        
        GC.SuppressFinalize(this);
    }

    private static readonly List<TUser> _users = new ();
    
    private static readonly List<IdentityUserClaim<TKey>> _userClaims = new();
    private static readonly List<IdentityUserLogin<TKey>> _logins = new();
    private static readonly List<IdentityUserToken<TKey>> _tokens = new();
    private static readonly List<IdentityUserRole<TKey>> _userRoles = new();
    // ReSharper disable once CollectionNeverUpdated.Local
   
    
    public Task<string?> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
    {
        return user != null ? Task.FromResult(user.Id?.ToString()) : throw new ArgumentNullException(nameof (user));
    }

    public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
    {
        return user != null ? Task.FromResult(user.UserName) : throw new ArgumentNullException(nameof (user));
    }

    public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        
        return Task.CompletedTask;
    }

    public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
    {
        return user != null ? Task.FromResult(user.NormalizedUserName) : throw new ArgumentNullException(nameof (user));
    }

    public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        
        return Task.CompletedTask;
    }

    public Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof (user));
        
        _users.Add(user);
        
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
    {
        _users.Remove(user);
        
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_users.SingleOrDefault(u => u.Id.Equals(userId) ))!;
    }

    public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return Task.FromResult(_users.AsQueryable().FirstOrDefault(x => x.NormalizedUserName == normalizedUserName))!;
    }

    public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
    {
        var list = _userClaims.Where(c => c.UserId.Equals(user.Id)).Select(c =>
            new Claim(c.ClaimType, c.ClaimValue)
        ).ToList();
        
        return Task.FromResult((IList<Claim>) list);
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

    public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        var matchedClaims = _userClaims.Where(c =>
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
                _userClaims.FirstOrDefault(
                    uc => uc.UserId.Equals(user.Id) && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value);
            if (entity != null)
            {
                _userClaims.Remove(entity);
            }
        }
        
        return Task.CompletedTask;
    }

    public Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        if (claim == null)
        {
            throw new ArgumentNullException(nameof(claim));
        }

        var ids = _userClaims.Where(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value).Select(c => c.UserId);
        
        return Task.FromResult<IList<TUser>>(_users.Where(i => ids.Contains(i.Id)).ToList());
    }

    public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        _logins.Add(new IdentityUserLogin<TKey>
        {
            UserId = user.Id,
            ProviderKey = login.ProviderKey,
            LoginProvider = login.LoginProvider,
            ProviderDisplayName = login.ProviderDisplayName
        });

        return Task.CompletedTask;
    }

    public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        var loginEntity =
            _logins.SingleOrDefault(
                l =>
                    l.ProviderKey == providerKey && l.LoginProvider == loginProvider &&
                    l.UserId.Equals(user.Id));
        
        if (loginEntity != null)
        {
            _logins.Remove(loginEntity);
        }
        
        return Task.CompletedTask;
    }

    public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
    {
        IList<UserLoginInfo> result = _logins
            .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName)).ToList();
        return Task.FromResult(result);
    }

    public Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        var login = _logins.SingleOrDefault(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
        if (login == null)
            return Task.FromResult<TUser>(null!);
        
        return Task.FromResult(_users.SingleOrDefault(u => u.Id.Equals(login.UserId)))!;
    }

    public Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        var role = InMemoryRoleStore<TRole, TKey>.RolesCollection.SingleOrDefault(r => r.Name == roleName);
        
        ArgumentNullException.ThrowIfNull(role);
        
        var identityRole = new IdentityUserRole<TKey>
        {
            UserId = user.Id,
            RoleId = role.Id
        };
        
        _userRoles.Add(identityRole);

        return Task.CompletedTask;
    }

    public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        var role = InMemoryRoleStore<TRole, TKey>.RolesCollection.SingleOrDefault(r => r.Name == roleName);
        
        ArgumentNullException.ThrowIfNull(role);

        var userRole = _userRoles.SingleOrDefault(u => u.UserId.Equals(user.Id) && u.RoleId.Equals(role.Id));
        if (userRole != null)
            _userRoles.Remove(userRole);

        return Task.CompletedTask;
    }

    public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
    {
        var ids = _userRoles.Where(u => u.UserId.Equals(user.Id)).Select(r => r.RoleId);

        var list = InMemoryRoleStore<TRole, TKey>.RolesCollection.Where(r => ids.Contains(r.Id)).Select(r => r.Name);

        return Task.FromResult((IList<string>)list.ToList());
    }

    public Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        var role = InMemoryRoleStore<TRole, TKey>.RolesCollection.SingleOrDefault(r => r.Name == roleName);
        if (role == null)
            return Task.FromResult(false);
        
        var any = _userRoles.Any(c => c.UserId.Equals(user.Id) && c.RoleId.Equals(role.Id));
        
        return Task.FromResult(any);
    }

    public Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        var role = InMemoryRoleStore<TRole, TKey>.RolesCollection.SingleOrDefault(r => r.Name == roleName);
        if (role == null)
            return Task.FromResult((IList<TUser>)new List<TUser>());
        
        var ids = _userRoles.Where(u => u.UserId.Equals(role.Id)).Select(r => r.RoleId);

        var list = _users.Where(r => ids.Contains(r.Id));

        return Task.FromResult((IList<TUser>)list.ToList());
    }

    public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        
        return Task.CompletedTask;
    }

    public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;

        return Task.CompletedTask;
    }

    public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.SecurityStamp);
    }

    public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
    {
        user.Email = email;

        return Task.CompletedTask;
    }

    public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;

        return Task.CompletedTask;
    }

    public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return Task.FromResult(_users.AsQueryable().FirstOrDefault(x => x.NormalizedEmail == normalizedEmail))!;
    }

    public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;

        return Task.CompletedTask;
    }

    public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
    {
        user.PhoneNumber = phoneNumber;

        return Task.CompletedTask;
    }

    public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PhoneNumber);
    }

    public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PhoneNumberConfirmed);
    }

    public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.PhoneNumberConfirmed = confirmed;

        return Task.CompletedTask;
    }

    public IQueryable<TUser> Users => _users.AsQueryable();

    public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
    {
        user.TwoFactorEnabled = enabled;

        return Task.CompletedTask;
    }

    public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.TwoFactorEnabled);
    }

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.LockoutEnd);
    }

    public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        user.LockoutEnd = lockoutEnd;

        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount += 1;

        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount = 0;

        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.LockoutEnabled);
    }

    public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
    {
        user.LockoutEnabled = enabled;

        return Task.CompletedTask;
    }
    
    public Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
    {
        var tokenEntity =
            _tokens.SingleOrDefault(
                l =>
                    l.Name == name && l.LoginProvider == loginProvider &&
                    l.UserId.Equals(user.Id));
        if (tokenEntity != null)
        {
            tokenEntity.Value = value;
        }
        else
        {
            _tokens.Add(new IdentityUserToken<TKey>
            {
                UserId = user.Id,
                LoginProvider = loginProvider,
                Name = name,
                Value = value
            });
        }
        return Task.FromResult(0);
    }

    public Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        var tokenEntity =
            _tokens.SingleOrDefault(
                l =>
                    l.Name == name && l.LoginProvider == loginProvider &&
                    l.UserId.Equals(user.Id));
        if (tokenEntity != null)
        {
            _tokens.Remove(tokenEntity);
        }
        return Task.FromResult(0);
    }

    public Task<string?> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        var tokenEntity =
            _tokens.SingleOrDefault(
                l =>
                    l.Name == name && l.LoginProvider == loginProvider &&
                    l.UserId.Equals(user.Id));
        return Task.FromResult(tokenEntity?.Value);
    }

    private const string AuthenticatorStoreLoginProvider = "[AspNetUserStore]";
    private const string AuthenticatorKeyTokenName = "AuthenticatorKey";
    private const string RecoveryCodeTokenName = "RecoveryCodes";

    public Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken)
    {
        return SetTokenAsync(user, AuthenticatorStoreLoginProvider, AuthenticatorKeyTokenName, key, cancellationToken);
    }

    public Task<string?> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken)
    {
        return GetTokenAsync(user, AuthenticatorStoreLoginProvider, AuthenticatorKeyTokenName, cancellationToken);
    }

    public Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
    {
        var mergedCodes = string.Join(";", recoveryCodes);
        return SetTokenAsync(user, AuthenticatorStoreLoginProvider, RecoveryCodeTokenName, mergedCodes, cancellationToken);
    }

    public async Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
    {
        var mergedCodes = await GetTokenAsync(user, AuthenticatorStoreLoginProvider, RecoveryCodeTokenName, cancellationToken) ?? "";
        var splitCodes = mergedCodes.Split(';');
        if (splitCodes.Contains(code))
        {
            var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
            await ReplaceCodesAsync(user, updatedCodes, cancellationToken);
            return true;
        }
        return false;
    }

    public async Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
    {
        var mergedCodes = await GetTokenAsync(user, AuthenticatorStoreLoginProvider, RecoveryCodeTokenName, cancellationToken) ?? "";
        if (mergedCodes.Length > 0)
        {
            return mergedCodes.Split(';').Length;
        }
        return 0;
    }
}