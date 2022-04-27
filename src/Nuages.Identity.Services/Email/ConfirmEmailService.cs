using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.Services.Email;

public class ConfirmEmailService : IConfirmEmailService
{
    private readonly NuagesUserManager _userManager;
    private readonly IIdentityEventBus _identityEventBus;

    public ConfirmEmailService(NuagesUserManager userManager, IIdentityEventBus identityEventBus)
    {
        _userManager = userManager;
        _identityEventBus = identityEventBus;
    }

    public async Task<bool> Confirm(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

        var res = await _userManager.ConfirmEmailAsync(user, code) == IdentityResult.Success;

        if (res)
        {
            await _identityEventBus.PutEvent(IdentityEvents.ConfirmEmailSuccess, user);
        }
        else
        {
            await _identityEventBus.PutEvent(IdentityEvents.ConfirmEmailFailed, user);
        }
        
        return res ;
    }
}

public interface IConfirmEmailService
{
    Task<bool> Confirm(string userId, string code);
}