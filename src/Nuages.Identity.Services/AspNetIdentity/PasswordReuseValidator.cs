using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Nuages.Identity.Services.AspNetIdentity;

public class PasswordReuseValidator<TUser, TKey> : IPasswordValidator<TUser> 
    where TKey : IEquatable<TKey> where  TUser :  NuagesApplicationUser<TKey>
{
    private readonly NuagesIdentityOptions _identityOptions;

    public PasswordReuseValidator(IOptions<NuagesIdentityOptions> identityOptions)
    {
        _identityOptions = identityOptions.Value;
    }
    
    public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
    {
        if (!_identityOptions.EnablePasswordHistory)
            return Task.FromResult(IdentityResult.Success);

        if (user.PasswordHistory != null)
        {
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var pwd in user.PasswordHistory.Passwords)
            {
                var res = manager.PasswordHasher.VerifyHashedPassword(user, pwd.Split("|").First(), password);
                if (res == PasswordVerificationResult.Success)
                {
                    return Task.FromResult(IdentityResult.Failed(new IdentityError { Code = "cantReusePassword", Description = "cantReusePassword" }));
                }
            }
        }

        return Task.FromResult(IdentityResult.Success);
    }
}