using System.ComponentModel;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.AspNetIdentity;

public class InMemoryUserStore<TUser, TRole, TKey> : 
    IUserClaimStore<TUser>,
    IUserStore<TUser>,
    IDisposable,
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
    private bool _disposed = false;
    
    public void Dispose()
    {
        _disposed = true;
    }

    private static List<TUser> _users = new ();
    
    private static List<IdentityUserClaim<TKey>> _userClaims = new();
    private static List<IdentityUserLogin<TKey>> _logins = new();
    private static List<IdentityUserToken<TKey>> _tokens = new();
    private static List<IdentityUserRole<TKey>> _roles = new();
    private static List<IdentityRoleClaim<TKey>> _roleClaims = new();
    
    public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        return user != null ? Task.FromResult(ConvertIdToString(user.Id)) : throw new ArgumentNullException(nameof (user));
    }

    public virtual string ConvertIdToString(TKey id) => (Equals(id,  default (TKey)!) ? null : id.ToString())!;
    public virtual TKey ConvertIdFromString(string? id) => (id == null ? default (TKey) : (TKey) TypeDescriptor.GetConverter(typeof (TKey)).ConvertFromInvariantString(id)!)!;

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(this.GetType().Name);
    }

    public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        return user != null ? Task.FromResult(user.UserName) : throw new ArgumentNullException(nameof (user));
    }

    public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.UserName = userName;
        
        return Task.CompletedTask;
    }

    public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        return user != null ? Task.FromResult(user.NormalizedUserName) : throw new ArgumentNullException(nameof (user));
    }

    public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.NormalizedUserName = normalizedName;
        
        return Task.CompletedTask;
    }

    public Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        if (user == null)
            throw new ArgumentNullException(nameof (user));
        
        _users.Add(user);
        
        return Task.FromResult(IdentityResult.Success);
        
    }

    public Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        _users.Remove(user);
        
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        
        return Task.FromResult(_users.SingleOrDefault(u => u.Id.Equals(userId) ))!;
    }

    public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        
        return Task.FromResult(_users.AsQueryable().FirstOrDefault(x => x.NormalizedUserName == normalizedUserName))!;
    }

    public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var list = _userClaims.Where(c => c.UserId.Equals(user.Id)).Select(c =>
            new Claim(c.ClaimType, c.ClaimValue)
        ).ToList();
        
        return Task.FromResult((IList<Claim>) list);
    }

    public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

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
        throw new NotImplementedException();
    }

    public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.PasswordHash = passwordHash;
        
        return Task.CompletedTask;
    }

    public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.SecurityStamp = stamp;

        return Task.CompletedTask;
    }

    public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(user.SecurityStamp);
    }

    public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.Email = email;

        return Task.CompletedTask;
    }

    public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.EmailConfirmed = confirmed;

        return Task.CompletedTask;
    }

    public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        
        return Task.FromResult(_users.AsQueryable().FirstOrDefault(x => x.NormalizedEmail == normalizedEmail))!;
    }

    public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.NormalizedEmail = normalizedEmail;

        return Task.CompletedTask;
    }

    public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.PhoneNumber = phoneNumber;

        return Task.CompletedTask;
    }

    public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(user.PhoneNumber);
    }

    public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(user.PhoneNumberConfirmed);
    }

    public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.PhoneNumberConfirmed = confirmed;

        return Task.CompletedTask;
    }

    public IQueryable<TUser> Users => _users.AsQueryable();

    public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.TwoFactorEnabled = enabled;

        return Task.CompletedTask;
    }

    public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(user.TwoFactorEnabled);
    }

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(user.LockoutEnd);
    }

    public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.LockoutEnd = lockoutEnd;

        return Task.CompletedTask;
    }

    public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.AccessFailedCount += 1;

        return Task.FromResult(user.AccessFailedCount);
    }

    public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.AccessFailedCount = 0;

        return Task.CompletedTask;
    }

    public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(user.AccessFailedCount);
    }

    public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Task.FromResult(user.LockoutEnabled);
    }

    public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        user.LockoutEnabled = enabled;

        return Task.CompletedTask;
    }

    public Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}