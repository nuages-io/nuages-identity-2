using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.InMemory;

// ReSharper disable once UnusedType.Global
public static class AspNetIdentityInMemoryExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static void AddInMemoryStorage(this IdentityBuilder builder)
    {
        builder.AddUserStore<InMemoryUserStore<NuagesApplicationUser, NuagesApplicationRole, string>>();
        builder.AddRoleStore<InMemoryRoleStore<NuagesApplicationRole, string>>();
    }
}