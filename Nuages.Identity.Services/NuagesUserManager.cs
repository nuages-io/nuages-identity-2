
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services;

public class NuagesUserManager : UserManager<NuagesApplicationUser> 
{
    public NuagesUserManager(IUserStore<NuagesApplicationUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<NuagesApplicationUser> passwordHasher, 
        IEnumerable<IUserValidator<NuagesApplicationUser>> userValidators, IEnumerable<IPasswordValidator<NuagesApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, 
        // ReSharper disable once ContextualLoggerProblem
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<NuagesApplicationUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

    public override Task<bool> IsEmailConfirmedAsync(NuagesApplicationUser user)
    {
        //TODO utiliser RequireConfirmedEmailGracePeriodInMinutes et la date du Email
        return base.IsEmailConfirmedAsync(user);
    }
    
}

