using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

using Nuages.AspNetIdentity.Core;


namespace Nuages.Identity.Services.Email;

public class ConfirmEmailService : IConfirmEmailService
{
    private readonly NuagesUserManager _userManager;

    public ConfirmEmailService(NuagesUserManager userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<bool> Confirm(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        return await _userManager.ConfirmEmailAsync(user, code) == IdentityResult.Success;
    }
}

public interface IConfirmEmailService
{
    Task<bool> Confirm(string userId, string code);
}