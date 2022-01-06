
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services;

public class NuagesUserManager : UserManager<NuagesApplicationUser> 
{
    private readonly NuagesIdentityOptions _nuagesIdentityOptions;

    public NuagesUserManager(IUserStore<NuagesApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<NuagesApplicationUser> passwordHasher, 
        IEnumerable<IUserValidator<NuagesApplicationUser>> userValidators, IEnumerable<IPasswordValidator<NuagesApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, 
        // ReSharper disable once ContextualLoggerProblem
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<NuagesApplicationUser>> logger, IOptions<NuagesIdentityOptions> nuagesIdentityOptions) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _nuagesIdentityOptions = nuagesIdentityOptions.Value;
    }

    public override async Task<bool> IsEmailConfirmedAsync(NuagesApplicationUser user)
    {
        var confirmed = await base.IsEmailConfirmedAsync(user);
        if (!confirmed)
        {
            if (user.EmailDateTime.HasValue)
            {
                var notVerifiedAfter = user.EmailDateTime.Value.AddMinutes(
                    _nuagesIdentityOptions.RequireConfirmedEmailGracePeriodInMinutes);
                                
                if (notVerifiedAfter < DateTime.UtcNow)
                {
                    return confirmed;
                }
            }
            else
            {
                return confirmed;
            }
        }

        return confirmed;
    }


    public override async Task<bool> IsPhoneNumberConfirmedAsync (NuagesApplicationUser user)
    {
        var confirmed = await base.IsPhoneNumberConfirmedAsync(user);
        if (!confirmed)
        {
            if (user.PhoneDateTime.HasValue)
            {
                var notVerifiedAfter = user.PhoneDateTime.Value.AddMinutes(
                    _nuagesIdentityOptions.RequireConfirmedPhoneGracePeriodInMinutes);
                                
                if (notVerifiedAfter < DateTime.UtcNow)
                {
                    return confirmed;
                }
            }
            else
            {
                return confirmed;
            }
        }

        return confirmed;
    }
    
    public override bool SupportsUserLockout
    {
        get
        {
            ThrowIfDisposed();

            if (!_nuagesIdentityOptions.SupportsUserLockout)
                return false;
            
            return Store is IUserLockoutStore<NuagesApplicationUser>;
        }
    }

    public override Task<IdentityResult> CreateAsync(NuagesApplicationUser user)
    {
        user.CreatedOn = DateTime.UtcNow;
        
        return base.CreateAsync(user);
    }

    public async Task<NuagesApplicationUser?> FindAsync(string userNameOrEmail)
    {
        NuagesApplicationUser? user = null;
        
        if (_nuagesIdentityOptions.SupportsUserName)
        {
            user = await FindByNameAsync(userNameOrEmail);
        }

        if (user == null)
            user = await FindByEmailAsync(userNameOrEmail);

        return user;
    }
}

