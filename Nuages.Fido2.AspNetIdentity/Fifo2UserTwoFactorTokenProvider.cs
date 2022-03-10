using Microsoft.AspNetCore.Identity;

namespace Nuages.Fido2.AspNetIdentity;


public class Fifo2UserTwoFactorTokenProvider<TUser, TKey> : IUserTwoFactorTokenProvider<TUser>  
    where TKey : IEquatable<TKey> 
    where TUser : IdentityUser<TKey>
{
    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult(true);
    }

    public Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult("fido2");
    }

    public Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult(true);
    }
}