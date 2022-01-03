
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services;

public class NuagesUserManager<TUser> : UserManager<TUser>  where TUser : class
{
    public NuagesUserManager(IUserStore<TUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<TUser> passwordHasher, 
        IEnumerable<IUserValidator<TUser>> userValidators, IEnumerable<IPasswordValidator<TUser>> passwordValidators, ILookupNormalizer keyNormalizer, 
        // ReSharper disable once ContextualLoggerProblem
        IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<TUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

  
}

