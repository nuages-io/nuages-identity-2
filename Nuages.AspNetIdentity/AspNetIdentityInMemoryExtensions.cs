using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity;

// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public static class AspNetIdentityInMemoryExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static void AddInMemoryStorage(this IdentityBuilder builder)
    {
        // builder.AddUserStore<InMemoryUserStore<NuagesApplicationUser, NuagesApplicationRole, string>>();
        // builder.AddRoleStore<InMemoryRoleStore<NuagesApplicationRole, string>>();
    }
}