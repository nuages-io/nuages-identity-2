using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services.AspNetIdentity;

public class NuagesUserManager : UserManager<NuagesApplicationUser> 
{
    private readonly NuagesIdentityOptions _nuagesIdentityOptions;

    public NuagesUserManager(IUserStore<NuagesApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<NuagesApplicationUser> passwordHasher, 
        IEnumerable<IUserValidator<NuagesApplicationUser>> userValidators, IEnumerable<IPasswordValidator<NuagesApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer,
        // ReSharper disable once ContextualLoggerProblem
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<NuagesApplicationUser>> logger, 
        IOptions<NuagesIdentityOptions> nuagesIdentityOptions) : 
        base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _nuagesIdentityOptions = nuagesIdentityOptions.Value;
    }

    
    public override bool SupportsUserLockout
    {
        get
        {
            ThrowIfDisposed();
    
            if (!_nuagesIdentityOptions.EnableUserLockout)
                return false;
            
            return Store is IUserLockoutStore<NuagesApplicationUser>;
        }
    }

    public override async Task<IdentityResult> CreateAsync(NuagesApplicationUser user)
    {
        var res = await base.CreateAsync(user);
       
        if (res.Succeeded)
        {
            user.CreatedOn = DateTime.UtcNow;
            user.LastPasswordChangedDate = user.CreatedOn;
            
            await UpdateAsync(user);
        }

        return res;
    }

    public async Task<NuagesApplicationUser?> FindAsync(string userNameOrEmail)
    {
        NuagesApplicationUser? user = null;
        
        if (_nuagesIdentityOptions.SupportsUserName)
        {
            user = await FindByNameAsync(userNameOrEmail);
        }

        // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
        if (user == null)
            user = await FindByEmailAsync(userNameOrEmail);

        return user;
    }

    protected override async Task<IdentityResult> UpdatePasswordHash(NuagesApplicationUser user, string newPassword, bool validatePassword)
    {
        var res = await base.UpdatePasswordHash(user, newPassword, validatePassword);

        if (res.Succeeded)
        {
            user.LastPasswordChangedDate = DateTime.UtcNow;
            await UpdateAsync(user);
        }

        return res;
    }

    public async Task<List<string>> GetRecoveryCodes(NuagesApplicationUser user)
    {
        var recoveryCode = await GetAuthenticationTokenAsync(user, "[AspNetUserStore]", "RecoveryCodes");
        return recoveryCode?.Split(";").ToList() ?? new List<string>();
    }
}

