using System.Text;
using Fido2NetLib;
using Microsoft.AspNetCore.Identity;
using Nuages.Fido2.Storage;

namespace Nuages.Fido2.AspNetIdentity;

public class Fido2UserStore<TUser> : IFido2UserStore 
                where TUser : class
{
    private readonly UserManager<TUser> _userManager;

    public Fido2UserStore(UserManager<TUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Fido2User?> GetUserAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user != null)
        {
            return new Fido2User
            {
                Name = userName,
                DisplayName = userName,
                Id = Encoding.UTF8.GetBytes(await _userManager.GetUserIdAsync(user))
            };
        }

        return null;
    }
}