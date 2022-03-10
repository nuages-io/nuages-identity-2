using Microsoft.AspNetCore.Identity;

namespace Nuages.Fido2.AspNetIdentity;

public class Fido2SignInManager<TUser>  : IFido2SignInManager 
    where TUser : class
{
    private readonly SignInManager<TUser> _signInManager;

    public Fido2SignInManager(SignInManager<TUser> signInManager)
    {
        _signInManager = signInManager;
    }
    
    public async Task<SignInResult> SignInAsync()
    {
        // complete sign-in
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        return await _signInManager.TwoFactorSignInAsync("FIDO2", string.Empty, false, false);
    }
}