using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.Login.MagicLink;

public class MagicLinkLoginProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser>
    where TUser : class
{
    public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult(false);
    }

    //We need to override this method as well.
    public override async Task<string> GetUserModifierAsync(string purpose, UserManager<TUser> manager, TUser user)
    {
        var userId = await manager.GetUserIdAsync(user);

        return "MagicLinkLogin:" + purpose + ":" + userId;
    }
}