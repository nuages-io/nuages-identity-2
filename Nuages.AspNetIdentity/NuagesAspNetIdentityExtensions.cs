using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity;

[ExcludeFromCodeCoverage]
public static class NuagesAspNetIdentityExtensions
{
    public static IdentityBuilder AddNuagesAspNetIdentity(this IServiceCollection services,
        Action<IdentityOptions> identityOptions)
    {
        var identityBuilder = services.AddIdentity<NuagesApplicationUser, NuagesApplicationRole>(identityOptions);

        identityBuilder
            .AddUserManager<NuagesUserManager>()
            .AddSignInManager<NuagesSignInManager>()
            .AddRoleManager<NuagesRoleManager>()
            .AddDefaultTokenProviders();

        return identityBuilder;

    }
}