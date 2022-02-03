using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity;

public static class NuagesAspNetIdentityExtensions
{
    public static IdentityBuilder AddNuagesAspNetIdentity(this IServiceCollection services,
        Action<IdentityOptions> identityOptions)
    {
        var identityBuilder = services.AddIdentity<NuagesApplicationUser, NuagesApplicationRole>(identityOptions);

        identityBuilder
            .AddDefaultTokenProviders()
            .AddUserManager<NuagesUserManager>()
            .AddSignInManager<NuagesSignInManager>()
            .AddRoleManager<NuagesRoleManager>();

        return identityBuilder;

    }
}