
using Microsoft.AspNetCore.Identity;
// ReSharper disable ContextualLoggerProblem

namespace Nuages.AspNetIdentity.Core;

// ReSharper disable once ClassNeverInstantiated.Global
public class NuagesRoleManager : RoleManager<NuagesApplicationRole<string>>
{
    public NuagesRoleManager(IRoleStore<NuagesApplicationRole<string>> store, 
        IEnumerable<IRoleValidator<NuagesApplicationRole<string>>> roleValidators, 
        ILookupNormalizer keyNormalizer, 
        IdentityErrorDescriber errors, 
        ILogger<RoleManager<NuagesApplicationRole<string>>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
    {
    }
    
}