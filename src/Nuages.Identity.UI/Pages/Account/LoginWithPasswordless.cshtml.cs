using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

public class LoginWithPasswordless : PageModel
{
    private readonly UIOptions _options;
    
    public string? ReturnUrl { get; set; }
    
    public LoginWithPasswordless(IOptions<UIOptions> options)
    {
        _options = options.Value;

    }

    

    public ActionResult OnGet(string? returnUrl = null)
    {
        if (!_options.EnablePasswordless)
            return Forbid();
        
        ReturnUrl = returnUrl;
        
        return Page();
    }
}