using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Core;

public static class NuagesAspNetIdentityExtensions
{
    public static IdentityBuilder AddNuagesAspNetIdentity<TUser, TRole>(this IServiceCollection services,
        Action<IdentityOptions> identityOptions) where TUser : class
        where TRole : class
    {
        var identityBuilder =
            services.AddIdentity<TUser, TRole>(identityOptions);

        identityBuilder
            .AddUserManager<NuagesUserManager>()
            .AddSignInManager<NuagesSignInManager>()
            .AddRoleManager<NuagesRoleManager>()
            .AddDefaultTokenProviders();

        return identityBuilder;
    }
}