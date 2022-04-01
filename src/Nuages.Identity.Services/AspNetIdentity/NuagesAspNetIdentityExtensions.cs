using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.AspNetIdentity;

public static class NuagesAspNetIdentityExtensions
{
    public static IdentityBuilder AddNuagesAspNetIdentity<TUser, TRole, TKey>(this IServiceCollection services,
        Action<IdentityOptions> identityOptions) where TKey : IEquatable<TKey> where  TUser :  NuagesApplicationUser<TKey>
        where TRole : class
    {
        var identityBuilder =
            services.AddIdentity<TUser, TRole>(identityOptions);

        identityBuilder
            .AddUserManager<NuagesUserManager>()
            .AddSignInManager<NuagesSignInManager>()
            .AddRoleManager<NuagesRoleManager>()
            .AddDefaultTokenProviders();

        services.AddScoped<IPasswordValidator<TUser>, PasswordReuseValidator<TUser, TKey>>();
        
        return identityBuilder;
    }
}