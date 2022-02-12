using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Stores;

public abstract class UserStoreBase <TUser, TRole, TKey,  TUserLogin, TUserToken, TUserRole>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
    where TUserRole : IdentityUserRole<TKey>, new()
{
    
    
    public abstract IQueryable<TUser> Users { get; }
    protected abstract IQueryable<TRole> Roles { get; }
    
    protected abstract IQueryable<TUserLogin> UsersLogins { get; }
    protected abstract IQueryable<TUserToken> UsersTokens { get; }
    protected abstract IQueryable<TUserRole> UsersRoles { get; }

    public abstract Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken);
    
    
    public Task<string?> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
    {
        return user != null ? Task.FromResult(user.Id?.ToString()) : throw new ArgumentNullException(nameof (user));
    }

    public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
    {
        return user != null ? Task.FromResult(user.UserName) : throw new ArgumentNullException(nameof (user));
    }

    public async Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;

        await SetNormalizedUserNameAsync(user, userName.ToUpper(), cancellationToken);

        
        await UpdateAsync(user, cancellationToken);
    }

    public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
    {
        return user != null ? Task.FromResult(user.NormalizedUserName) : throw new ArgumentNullException(nameof (user));
    }

    public async Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        
        await UpdateAsync(user, cancellationToken);
    }

    protected static TKey StringToKey(string id)
    {
        return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id)!;
    }
    
    public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(Users.SingleOrDefault(u => u.Id.Equals(userId) ))!;
    }

    public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        // ReSharper disable once SpecifyStringComparison
        return Task.FromResult(Users.FirstOrDefault(x => x.NormalizedUserName.ToUpper()  == normalizedUserName.ToUpper() ))!;
    }
    
  
    
    
    public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
    {
        IList<UserLoginInfo> result = UsersLogins.Where(u => u.UserId.Equals(user.Id)).ToList()
            .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName)).ToList();
        return Task.FromResult(result);
    }

    public Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        var login = UsersLogins.SingleOrDefault(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
        return login == null ? Task.FromResult<TUser>(null!) : Task.FromResult(Users.SingleOrDefault(u => u.Id.Equals(login.UserId)))!;
    }
    
     public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
    {
        var query = from p in UsersRoles.AsQueryable()
                .Where(u => u.UserId.Equals(user.Id))
            join o in Roles on p.RoleId equals o.Id
            select o.Name;

        return Task.FromResult((IList<string>)query.ToList());
    }

     // ReSharper disable once UnusedParameter.Global
     protected abstract Task<TRole?> FindRoleByNameAsync(string normalizedName, CancellationToken cancellationToken);
     
    public async Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = await FindRoleByNameAsync(normalizedRoleName, cancellationToken);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        return role != null && UsersRoles.Any(c => c.UserId.Equals(user.Id) && c.RoleId.Equals(role.Id));
    }

    public Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = Roles.SingleOrDefault(r => r.NormalizedName == normalizedRoleName);
        if (role == null)
            return Task.FromResult((IList<TUser>)new List<TUser>());
        
        var query = from p 
                in UsersRoles.AsQueryable().Where(u => u.RoleId.Equals(role.Id))
            join o in Users on p.UserId equals o.Id
            select o;

        return Task.FromResult((IList<TUser>)query.ToList());
    }

    public async Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        
        await UpdateAsync(user, cancellationToken);
    }

    public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public async Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
    {
        user.TwoFactorEnabled = enabled;

        await UpdateAsync(user, cancellationToken);
    }

    public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.TwoFactorEnabled);
    }

    public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.LockoutEnd);
    }

    public async Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        user.LockoutEnd = lockoutEnd;

        await UpdateAsync(user, cancellationToken);
    }

    public async Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount += 1;

        await UpdateAsync(user, cancellationToken);

        return user.AccessFailedCount;
    }

    public async Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        user.AccessFailedCount = 0;

        await UpdateAsync(user, cancellationToken);
    }

    public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.AccessFailedCount);
    }

    public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.LockoutEnabled);
    }

    public async Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
    {
        user.LockoutEnabled = enabled;

        await UpdateAsync(user, cancellationToken);
    }
    
    public async Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
    {
        user.Email = email;

        await SetNormalizedEmailAsync(user, email.ToUpper(), cancellationToken);
        
        await UpdateAsync(user, cancellationToken);
    }

    public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public async Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;

        await UpdateAsync(user, cancellationToken);
    }

    public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        // ReSharper disable once SpecifyStringComparison
        return Task.FromResult(Users.AsQueryable().FirstOrDefault(x => x.NormalizedEmail.ToUpper() == normalizedEmail.ToUpper() ))!;
    }

    public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public async Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;

        await UpdateAsync(user, cancellationToken);
    }

    public async Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
    {
        user.PhoneNumber = phoneNumber;

        await UpdateAsync(user, cancellationToken);
    }

    public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PhoneNumber);
    }

    public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PhoneNumberConfirmed);
    }

    public async Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.PhoneNumberConfirmed = confirmed;

        await UpdateAsync(user, cancellationToken);
    }
    
    public Task<string?> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        var tokenEntity =
            UsersTokens.SingleOrDefault(
                l =>
                    l.Name == name && l.LoginProvider == loginProvider &&
                    l.UserId.Equals(user.Id));
        return Task.FromResult(tokenEntity?.Value);
    }

   
    
    public Task<string?> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken)
    {
        return GetTokenAsync(user, AuthenticatorInfo.AuthenticatorStoreLoginProvider, AuthenticatorInfo.AuthenticatorKeyTokenName, cancellationToken);
    }
    
    public async Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
    {
        var mergedCodes = await GetTokenAsync(user, AuthenticatorInfo.AuthenticatorStoreLoginProvider, AuthenticatorInfo.RecoveryCodeTokenName, cancellationToken) ?? "";
        return mergedCodes.Length > 0 ? mergedCodes.Split(';').Length : 0;
    }
    
    public async Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
    {
        var tokenEntity =
            UsersTokens.SingleOrDefault(
                l =>
                    l.Name == name && l.LoginProvider == loginProvider &&
                    l.UserId.Equals(user.Id));
        if (tokenEntity != null)
        {
            tokenEntity.Value = value;

            await UpdateUserTokenAsync(tokenEntity);
        }
        else
        {
            await AddUserTokenAsync(new TUserToken
            {
                UserId = user.Id,
                LoginProvider = loginProvider,
                Name = name,
                Value = value
            });
        }
    }

    public Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken)
    {
        return SetTokenAsync(user, AuthenticatorInfo.AuthenticatorStoreLoginProvider, AuthenticatorInfo.AuthenticatorKeyTokenName, key, cancellationToken);
    }

    public Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
    {
        var mergedCodes = string.Join(";", recoveryCodes);
        return SetTokenAsync(user, AuthenticatorInfo.AuthenticatorStoreLoginProvider, AuthenticatorInfo.RecoveryCodeTokenName, mergedCodes, cancellationToken);
    }

    public async Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
    {
        var mergedCodes = await GetTokenAsync(user, AuthenticatorInfo.AuthenticatorStoreLoginProvider, AuthenticatorInfo.RecoveryCodeTokenName, cancellationToken) ?? "";
        var splitCodes = mergedCodes.Split(';');
        if (splitCodes.Contains(code))
        {
            var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
            await ReplaceCodesAsync(user, updatedCodes, cancellationToken);
            return true;
        }
        return false;
    }

   

  
    
    public async Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        var tokenEntity =
            UsersTokens.SingleOrDefault(
                l =>
                    l.Name == name && l.LoginProvider == loginProvider &&
                    l.UserId.Equals(user.Id));
        
        if (tokenEntity != null)
        {
            await RemoveTokenAsync(tokenEntity);
        }
    }
    
    public async Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = await FindRoleByNameAsync(normalizedRoleName, cancellationToken);
        
        ArgumentNullException.ThrowIfNull(role);

        var userRole = UsersRoles.SingleOrDefault(u => u.UserId.Equals(user.Id) && u.RoleId.Equals(role.Id));
        if (userRole != null)
            await RemoveUserRoleAsync(userRole);
    }
    
    public async Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = await FindRoleByNameAsync(normalizedRoleName, cancellationToken);
        
        ArgumentNullException.ThrowIfNull(role);
        
        var identityRole = new TUserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        };
        
        await AddUserRoleAsync(identityRole);
    }

    public async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        var loginEntity =
            UsersLogins.SingleOrDefault(
                l =>
                    l.ProviderKey == providerKey && l.LoginProvider == loginProvider &&
                    l.UserId.Equals(user.Id));
        
        if (loginEntity != null)
        {
            await RemoveUserLoginAsync(loginEntity);
        }
        
    }
    
    
    protected abstract Task AddUserTokenAsync(TUserToken token);
    protected abstract Task UpdateUserTokenAsync(TUserToken token);
    protected abstract Task RemoveTokenAsync(TUserToken token);
    protected abstract Task RemoveUserRoleAsync(TUserRole role);
    protected abstract Task AddUserRoleAsync(TUserRole role);
    protected abstract Task RemoveUserLoginAsync(TUserLogin login);
}