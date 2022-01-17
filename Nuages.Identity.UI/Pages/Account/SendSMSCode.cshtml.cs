using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.UI.Pages.Account;

public class LoginWithSMS : PageModel
{
    private readonly NuagesSignInManager _signInManager;

    public LoginWithSMS(NuagesSignInManager signInManager)
    {
        _signInManager = signInManager;
    }
    
    public async Task<IActionResult> OnGet(string returnUrl = null)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        ReturnUrl = returnUrl;
        
        return Page();
    }

    public string ReturnUrl { get; set; }
}