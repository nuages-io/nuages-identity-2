using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Nuages.Identity.UI.Setup;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

public class LoginWithMagicLink : PageModel
{
    private readonly UIOptions _options;
    
    public string? ReturnUrl { get; set; }
    
    public LoginWithMagicLink(IOptions<UIOptions> options)
    {
        _options = options.Value;

    }

    

    public ActionResult OnGet(string? returnUrl = null)
    {
        if (!_options.EnableMagicLink)
            return Forbid();
        
        ReturnUrl = returnUrl;
        
        return Page();
    }
}