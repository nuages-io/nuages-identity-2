
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Nuages.AspNetIdentity.Stores.Mongo;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once UnusedType.Global
public class MongoUserStore<TUser, TRole, TKey> : UserStoreBase<TUser, TRole, TKey, 
        MongoIdentityUserLogin<TKey>, MongoIdentityUserToken<TKey>, MongoIdentityUserRole<TKey>>,
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

    private readonly IdentityErrorDescriber _errorDescriber = new ();
    
    // ReSharper disable once StaticMemberInGenericType
    public static ReplaceOptions ReplaceOptions { get; } = new();
    // ReSharper disable once StaticMemberInGenericType
    public static DeleteOptions DeleteOptions  { get; } = new();
    // ReSharper disable once StaticMemberInGenericType
    public static InsertOneOptions InsertOneOptions  { get; } = new();
    // ReSharper disable once CollectionNeverUpdated.Local
   
    public MongoUserStore(IOptions<MongoIdentityOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.Database);

        UsersCollection = database.GetCollection<TUser>("AspNetUsers");
        RolesCollection = database.GetCollection<TRole>("AspNetRoles");
        UsersClaimsCollection = database.GetCollection<MongoIdentityUserClaim<TKey>>("AspNetUserClaims");
        UsersLoginsCollection = database.GetCollection<MongoIdentityUserLogin<TKey>>("AspNetUserLogins");
        UsersTokensCollection = database.GetCollection<MongoIdentityUserToken<TKey>>("AspNetUserTokens");
        UsersRolesCollection = database.GetCollection<MongoIdentityUserRole<TKey>>("AspNetUserRoles");
    }
    
    private  IMongoCollection<TUser> UsersCollection { get; }
    private  IMongoCollection<TRole> RolesCollection { get; }
    private  IMongoCollection<MongoIdentityUserClaim<TKey>> UsersClaimsCollection { get; }
    private  IMongoCollection<MongoIdentityUserLogin<TKey>> UsersLoginsCollection { get; }
    private  IMongoCollection<MongoIdentityUserToken<TKey>> UsersTokensCollection { get; }
    private  IMongoCollection<MongoIdentityUserRole<TKey>> UsersRolesCollection { get; }
    
    public override IQueryable<TUser> Users => UsersCollection.AsQueryable();
    protected override IQueryable<TRole> Roles => RolesCollection.AsQueryable();
    public IQueryable<MongoIdentityUserClaim<TKey>> UsersClaims => UsersClaimsCollection.AsQueryable();
    protected override IQueryable<MongoIdentityUserLogin<TKey>> UsersLogins => UsersLoginsCollection.AsQueryable();
    protected override IQueryable<MongoIdentityUserToken<TKey>> UsersTokens => UsersTokensCollection.AsQueryable();
    protected override IQueryable<MongoIdentityUserRole<TKey>> UsersRoles => UsersRolesCollection.AsQueryable();
    
    // public Task<string?> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return user != null ? Task.FromResult(user.Id?.ToString()) : throw new ArgumentNullException(nameof (user));
    // }
    //
    // public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return user != null ? Task.FromResult(user.UserName) : throw new ArgumentNullException(nameof (user));
    // }
    //
    // public async Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
    // {
    //     user.UserName = userName;
    //
    //     await UpdateAsync(user, cancellationToken);
    // }
    //
    // public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return user != null ? Task.FromResult(user.NormalizedUserName) : throw new ArgumentNullException(nameof (user));
    // }
    //
    // public async Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
    // {
    //     user.NormalizedUserName = normalizedName;
    //     
    //     await UpdateAsync(user, cancellationToken);
    // }
    //
    // private static TKey StringToKey(string id)
    // {
    //     return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id)!;
    // }
    //
    public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
    {
        user.Id = StringToKey(ObjectId.GenerateNewId().ToString());

        var email = await GetEmailAsync(user, cancellationToken);
        await SetNormalizedEmailAsync(user, email.ToUpper(), cancellationToken);

        var userName = await GetUserNameAsync(user, cancellationToken);
        if (string.IsNullOrEmpty(userName))
        {
            await SetUserNameAsync(user, email, cancellationToken);
            userName = email;
        }
        
        await SetNormalizedUserNameAsync(user,userName.ToUpper(), cancellationToken);
        
        await UsersCollection.InsertOneAsync(user, null, cancellationToken);
        
        return IdentityResult.Success;
    }

    public override async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
    {
        var currentConcurrencyStamp = user.ConcurrencyStamp;
        user.ConcurrencyStamp = Guid.NewGuid().ToString();

        var result = await UsersCollection.ReplaceOneAsync(x => x.Id.Equals(user.Id) &&
                                                                x.ConcurrencyStamp.Equals(currentConcurrencyStamp), 
            user, ReplaceOptions, cancellationToken);

        return ReturnUpdateResult(result);
    }

    protected override Task<TRole?> FindRoleByNameAsync(string normalizedName, CancellationToken cancellationToken)
    {
        // ReSharper disable once SpecifyStringComparison
        return Task.FromResult(Roles.SingleOrDefault(r => r.NormalizedName.ToUpper() == normalizedName.ToUpper()));
    }

    protected override async Task AddUserTokenAsync(MongoIdentityUserToken<TKey> token)
    {
        await UsersTokensCollection.InsertOneAsync(token);
    }

    protected override async Task UpdateUserTokenAsync(MongoIdentityUserToken<TKey> token)
    {
        await UsersTokensCollection.ReplaceOneAsync(t => t.Id.Equals(token.Id), token);
    }

    protected override async Task RemoveTokenAsync(MongoIdentityUserToken<TKey> token)
    {
        await UsersTokensCollection.DeleteOneAsync(t => t.Id.Equals(token.Id));
    }

    protected override async Task RemoveUserRoleAsync(MongoIdentityUserRole<TKey> role)
    {
        await UsersRolesCollection.DeleteOneAsync(r => r.Id.Equals(role.Id));
    }

    protected override async Task AddUserRoleAsync(MongoIdentityUserRole<TKey> role)
    {
        await UsersRolesCollection.InsertOneAsync(role);
    }

    protected override async Task RemoveUserLoginAsync(MongoIdentityUserLogin<TKey> login)
    {
        await UsersLoginsCollection.DeleteOneAsync(l => l.Id.Equals(login.Id));
    }

    
    private IdentityResult ReturnUpdateResult(ReplaceOneResult result)
    {
        if (!result.IsAcknowledged && result.ModifiedCount == 0)
        {
            return IdentityResult.Failed(_errorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
    {
        var deleteResult = await UsersCollection.DeleteOneAsync(u => u.Id.Equals(user.Id), DeleteOptions, cancellationToken);

        return ReturnDeleteResult(deleteResult);
    }
    
    
    
    private IdentityResult ReturnDeleteResult(DeleteResult result)
    {
        if (result.IsAcknowledged || result.DeletedCount != 0L)
            return IdentityResult.Success;

        return IdentityResult.Failed(_errorDescriber.ConcurrencyFailure());
    }


    // public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(Users.SingleOrDefault(u => u.Id.Equals(userId) ))!;
    // }
    //
    // public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    // {
    //     // ReSharper disable once SpecifyStringComparison
    //     return Task.FromResult(Users.FirstOrDefault(x => x.NormalizedUserName.ToUpper() == normalizedUserName.ToUpper()))!;
    // }

    public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
    {
        var list = UsersClaims.Where(c => c.UserId.Equals(user.Id)).ToList().Select(c =>
            new Claim(c.Type, c.Value)
        ).ToList();
        
        return Task.FromResult((IList<Claim>) list);
    }

    public  async  Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        var list = claims.Select(claim => new MongoIdentityUserClaim<TKey> 
            { Id = ObjectId.GenerateNewId().ToString(), 
                UserId = user.Id, 
                Type = claim.Type, 
                Value = claim.Value }).ToList();
    
        await UsersClaimsCollection.InsertManyAsync(list, null, cancellationToken);
    
    }

    public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        var matchedClaims = UsersClaims.Where(c =>
            c.Type == claim.Type && c.Value == claim.Value && user.Id.Equals(user.Id));

        foreach (var matchedClaim in matchedClaims)
        {
            matchedClaim.Value = newClaim.Value;
            matchedClaim.Type = newClaim.Type;

            await UsersClaimsCollection.ReplaceOneAsync(c => c.Id == matchedClaim.Id, matchedClaim, ReplaceOptions, cancellationToken);
        }
    }

    public async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        foreach (var claim in claims)
        {
            await UsersClaimsCollection.DeleteOneAsync(uc => uc.UserId.Equals(user.Id) && uc.Type == claim.Type && uc.Value == claim.Value,  cancellationToken);
        }
    }

    public  Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        var query = from p in 
                UsersClaims.AsQueryable().Where(u => u.Type == claim.Type)
            join o in Users on p.UserId equals o.Id 
            select o;
        
        return  Task.FromResult((IList<TUser>) query.ToList());
    }

    public async Task AddLoginAsync(TUser user, UserLoginInfo loginInfo, CancellationToken cancellationToken)
    {
        var login = new MongoIdentityUserLogin<TKey>
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = user.Id,
            ProviderKey = loginInfo.ProviderKey,
            LoginProvider = loginInfo.LoginProvider,
            ProviderDisplayName = loginInfo.ProviderDisplayName
        };
        
        await UsersLoginsCollection.InsertOneAsync(login, InsertOneOptions, cancellationToken);
    }

    // public async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    // {
    //     await UsersLoginsCollection.DeleteOneAsync(l =>
    //         l.ProviderKey == providerKey && l.LoginProvider == loginProvider &&
    //         l.UserId.Equals(user.Id), DeleteOptions, cancellationToken);
    // }
    //
    // public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     IList<UserLoginInfo> result = UsersLogins.Where(l => l.UserId.Equals(user.Id)).ToList()
    //         .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName)).ToList();
    //     return Task.FromResult(result);
    // }
    //
    // public Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    // {
    //     var login = UsersLogins.SingleOrDefault(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
    //     if (login == null)
    //         return Task.FromResult<TUser>(null!);
    //     
    //     return Task.FromResult(Users.SingleOrDefault(u => u.Id.Equals(login.UserId)))!;
    // }
    //
    // public async Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken)
    // {
    //     var role = Roles.SingleOrDefault(r => r.NormalizedName.ToUpper()  == normalizedRoleName.ToUpper() );
    //     
    //     ArgumentNullException.ThrowIfNull(role);
    //     
    //     var identityRole = new MongoIdentityUserRole<TKey>
    //     {
    //         Id = ObjectId.GenerateNewId().ToString(),
    //         UserId = user.Id,
    //         RoleId = role.Id
    //     };
    //     
    //     await UsersRolesCollection.InsertOneAsync(identityRole, InsertOneOptions, cancellationToken);
    // }
    //
    // public async Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken)
    // {
    //     var role = Roles.SingleOrDefault(r => r.NormalizedName.ToUpper()  == normalizedRoleName.ToUpper() );
    //     
    //     ArgumentNullException.ThrowIfNull(role);
    //
    //     await UsersRolesCollection.DeleteOneAsync(u => u.UserId.Equals(user.Id) && u.RoleId.Equals(role.Id), DeleteOptions, cancellationToken);
    // }
    //
    // public Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
    // {
    //
    //     var query = from p in UsersRoles.AsQueryable()
    //             .Where(u => u.UserId.Equals(user.Id))
    //         join o in Roles on p.RoleId equals o.Id
    //         select o.Name;
    //
    //     return Task.FromResult((IList<string>)query.ToList());
    // }
    //
    // public Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken)
    // {
    //     var role = Roles.SingleOrDefault(r => r.NormalizedName.ToUpper()  == normalizedRoleName.ToUpper() );
    //     if (role == null)
    //         return Task.FromResult(false);
    //     
    //     var any = UsersRoles.Any(c => c.UserId.Equals(user.Id) && c.RoleId.Equals(role.Id));
    //     
    //     return Task.FromResult(any);
    // }
    //
    // public Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
    // {
    //     var role = Roles.SingleOrDefault(r => r.NormalizedName == normalizedRoleName);
    //     if (role == null)
    //         return Task.FromResult((IList<TUser>)new List<TUser>());
    //     
    //     var query = from p 
    //             in UsersRoles.AsQueryable().Where(u => u.RoleId.Equals(role.Id))
    //         join o in Users on p.UserId equals o.Id
    //         select o;
    //
    //     return Task.FromResult((IList<TUser>)query.ToList());
    // }
    //
    // public async Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
    // {
    //     user.PasswordHash = passwordHash;
    //     
    //     await UpdateAsync(user, cancellationToken);
    // }
    //
    // public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(user.PasswordHash);
    // }
    //
    // public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    // }

    
    public async Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = stamp;

        await UpdateAsync(user, cancellationToken);
    }

    
    public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.SecurityStamp);
    }
    //
    // public async Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
    // {
    //     user.Email = email;
    //
    //     await UpdateAsync(user, cancellationToken);
    // }
    //
    // public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(user.Email);
    // }
    //
    // public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(user.EmailConfirmed);
    // }
    //
    // public async Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
    // {
    //     user.EmailConfirmed = confirmed;
    //
    //     await UpdateAsync(user, cancellationToken);
    // }
    //
    // public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(Users.AsQueryable().FirstOrDefault(x => x.NormalizedEmail.ToUpper()  == normalizedEmail.ToUpper() ))!;
    // }
    //
    // public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(user.NormalizedEmail);
    // }
    //
    // public async Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
    // {
    //     user.NormalizedEmail = normalizedEmail;
    //
    //     await UpdateAsync(user, cancellationToken);
    // }
    //
    // public async Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
    // {
    //     user.PhoneNumber = phoneNumber;
    //
    //     await UpdateAsync(user, cancellationToken);
    // }
    //
    // public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(user.PhoneNumber);
    // }
    //
    // public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(user.PhoneNumberConfirmed);
    // }
    //
    // public async Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
    // {
    //     user.PhoneNumberConfirmed = confirmed;
    //
    //     await UpdateAsync(user, cancellationToken);
    // }
    //
    // public async Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
    // {
    //     user.TwoFactorEnabled = enabled;
    //
    //     await UpdateAsync(user, cancellationToken);
    // }
    //
    // public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(user.TwoFactorEnabled);
    // }
    //
    // public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(user.LockoutEnd);
    // }
    //
    // public async Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    // {
    //     user.LockoutEnd = lockoutEnd;
    //
    //     await UpdateAsync(user, cancellationToken);
    // }
    //
    // public async Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     user.AccessFailedCount += 1;
    //
    //     await UpdateAsync(user, cancellationToken);
    //     
    //     return user.AccessFailedCount;
    // }
    //
    // public async Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     user.AccessFailedCount = 0;
    //
    //     await UpdateAsync(user, cancellationToken);
    // }
    //
    // public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(user.AccessFailedCount);
    // }
    //
    // public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(user.LockoutEnabled);
    // }
    //
    // public async Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
    // {
    //     user.LockoutEnabled = enabled;
    //
    //     await UpdateAsync(user, cancellationToken);
    // }
    //
    // public async Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
    // {
    //     var tokenEntity =
    //         UsersTokens.SingleOrDefault(
    //             l =>
    //                 l.Name == name && l.LoginProvider == loginProvider &&
    //                 l.UserId.Equals(user.Id));
    //     if (tokenEntity != null)
    //     {
    //         tokenEntity.Value = value;
    //
    //         await UsersTokensCollection.ReplaceOneAsync(t => t.Id == tokenEntity.Id, tokenEntity, ReplaceOptions, cancellationToken);
    //     }
    //     else
    //     {
    //         await UsersTokensCollection.InsertOneAsync(new MongoIdentityUserToken<TKey>
    //         {
    //             Id = ObjectId.GenerateNewId().ToString(),
    //             UserId = user.Id,
    //             LoginProvider = loginProvider,
    //             Name = name,
    //             Value = value
    //         }, InsertOneOptions, cancellationToken);
    //     }
    // }
    //
    // public async Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    // {
    //     await UsersTokensCollection.DeleteOneAsync(l =>
    //         l.Name == name && l.LoginProvider == loginProvider &&
    //         l.UserId.Equals(user.Id), DeleteOptions, cancellationToken);
    // }
    //
    // public Task<string?> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    // {
    //     var tokenEntity =
    //         UsersTokens.SingleOrDefault(
    //             l =>
    //                 l.Name == name && l.LoginProvider == loginProvider &&
    //                 l.UserId.Equals(user.Id));
    //     return Task.FromResult(tokenEntity?.Value);
    // }
    //
    // private const string AuthenticatorStoreLoginProvider = "[AspNetUserStore]";
    // private const string AuthenticatorKeyTokenName = "AuthenticatorKey";
    
    
    //private const string RecoveryCodeTokenName = "RecoveryCodes";

    // public Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken)
    // {
    //     return SetTokenAsync(user, AuthenticatorStoreLoginProvider, AuthenticatorKeyTokenName, key, cancellationToken);
    // }
    //
    // public Task<string?> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     return GetTokenAsync(user, AuthenticatorStoreLoginProvider, AuthenticatorKeyTokenName, cancellationToken);
    // }
    //
    // public Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
    // {
    //     var mergedCodes = string.Join(";", recoveryCodes);
    //     return SetTokenAsync(user, AuthenticatorStoreLoginProvider, RecoveryCodeTokenName, mergedCodes, cancellationToken);
    // }
    //
    // public async Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
    // {
    //     var mergedCodes = await GetTokenAsync(user, AuthenticatorStoreLoginProvider, RecoveryCodeTokenName, cancellationToken) ?? "";
    //     var splitCodes = mergedCodes.Split(';');
    //     if (splitCodes.Contains(code))
    //     {
    //         var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
    //         await ReplaceCodesAsync(user, updatedCodes, cancellationToken);
    //         return true;
    //     }
    //     return false;
    // }
    //
    // public async Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
    // {
    //     var mergedCodes = await GetTokenAsync(user, AuthenticatorStoreLoginProvider, RecoveryCodeTokenName, cancellationToken) ?? "";
    //     if (mergedCodes.Length > 0)
    //     {
    //         return mergedCodes.Split(';').Length;
    //     }
    //     return 0;
    // }
}