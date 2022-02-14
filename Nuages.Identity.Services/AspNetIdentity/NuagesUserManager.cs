using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

// ReSharper disable ContextualLoggerProblem

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services.AspNetIdentity;

public class NuagesUserManager : UserManager<NuagesApplicationUser<string>>
{
    private readonly NuagesIdentityOptions _nuagesIdentityOptions;

    public NuagesUserManager(IUserStore<NuagesApplicationUser<string>> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<NuagesApplicationUser<string>> passwordHasher,
        IEnumerable<IUserValidator<NuagesApplicationUser<string>>> userValidators,
        IEnumerable<IPasswordValidator<NuagesApplicationUser<string>>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors, IServiceProvider services,
        ILogger<UserManager<NuagesApplicationUser<string>>> logger,
        IOptions<NuagesIdentityOptions> nuagesIdentityOptions) :
        base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors,
            services, logger)
    {
        _nuagesIdentityOptions = nuagesIdentityOptions.Value;
    }

    public override async Task<IdentityResult> CreateAsync(NuagesApplicationUser<string> user)
    {
        var res = await base.CreateAsync(user);

        if (res.Succeeded)
        {
            user.CreatedOn = DateTime.UtcNow;
            user.LastPasswordChangedDate = user.CreatedOn;

            await UpdateAsync(user);
        }

        return res;
    }

    public async Task<NuagesApplicationUser<string>?> FindAsync(string userNameOrEmail)
    {
        var user = await FindByNameAsync(userNameOrEmail);

        if (user == null && _nuagesIdentityOptions.SupportsLoginWithEmail)
            user = await FindByEmailAsync(userNameOrEmail);

        return user;
    }

    public override async Task<IdentityResult> ChangePasswordAsync(NuagesApplicationUser<string> user,
        string currentPassword, string newPassword)
    {
        var res = await base.ChangePasswordAsync(user, currentPassword, newPassword);
        if (res.Succeeded)
        {
            user.LastPasswordChangedDate = DateTime.UtcNow;
            return await UpdateAsync(user);
        }

        return res;
    }

    public override async Task<IdentityResult> AddPasswordAsync(NuagesApplicationUser<string> user, string password)
    {
        var res = await base.AddPasswordAsync(user, password);
        if (res.Succeeded)
        {
            user.LastPasswordChangedDate = DateTime.UtcNow;
            return await UpdateAsync(user);
        }

        return res;
    }
}