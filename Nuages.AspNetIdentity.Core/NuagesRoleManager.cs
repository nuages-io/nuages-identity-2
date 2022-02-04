using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Core;

// ReSharper disable once ClassNeverInstantiated.Global
public class NuagesRoleManager : RoleManager<NuagesApplicationRole>
{
    [ExcludeFromCodeCoverage]
    public NuagesRoleManager(IRoleStore<NuagesApplicationRole> store, 
        IEnumerable<IRoleValidator<NuagesApplicationRole>> roleValidators, 
        ILookupNormalizer keyNormalizer, 
        IdentityErrorDescriber errors, 
        ILogger<RoleManager<NuagesApplicationRole>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
    {
    }
}