using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.AspNetIdentity;

// ReSharper disable once ClassNeverInstantiated.Global
public class NuagesRoleManager : RoleManager<NuagesApplicationRole>
{
    public NuagesRoleManager(IRoleStore<NuagesApplicationRole> store, 
        IEnumerable<IRoleValidator<NuagesApplicationRole>> roleValidators, 
        ILookupNormalizer keyNormalizer, 
        IdentityErrorDescriber errors, 
        ILogger<RoleManager<NuagesApplicationRole>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
    {
    }
}