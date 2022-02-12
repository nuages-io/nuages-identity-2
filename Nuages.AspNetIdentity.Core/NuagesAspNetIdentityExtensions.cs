using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Core;

public static class NuagesAspNetIdentityExtensions
{
    public static IdentityBuilder AddNuagesAspNetIdentity(this IServiceCollection services,
        Action<IdentityOptions> identityOptions)
    {
        var identityBuilder =
            services.AddIdentity<NuagesApplicationUser<string>, NuagesApplicationRole<string>>(identityOptions);

        identityBuilder
            .AddUserManager<NuagesUserManager>()
            .AddSignInManager<NuagesSignInManager>()
            .AddRoleManager<NuagesRoleManager>()
            .AddDefaultTokenProviders();

        return identityBuilder;
    }
}