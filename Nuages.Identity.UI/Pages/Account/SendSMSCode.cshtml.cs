using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

using Nuages.AspNetIdentity.Core;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.Pages.Account;

// ReSharper disable once InconsistentNaming
public class LoginWithSMS : PageModel
{
    private readonly NuagesSignInManager _signInManager;
    private readonly UIOptions _options;

    public LoginWithSMS(NuagesSignInManager signInManager, IOptions<UIOptions> options)
    {
        _signInManager = signInManager;
        _options = options.Value;
    }
    
    public async Task<IActionResult> OnGet(string? returnUrl = null)
    {
        if (!_options.EnablePhoneFallback)
        {
            return Forbid();
        }
        
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
        }

        ReturnUrl = returnUrl ?? "~/";
        
        return Page();
    }

    public string? ReturnUrl { get; set; }
}