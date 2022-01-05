using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

[AllowAnonymous]
public class Login : PageModel
{
    private readonly IStringLocalizer _stringLocalizer;

    public string Lang { get; set; } = "en-CA";
    public string? ReturnUrl { get; set; }
    public string UserNamePlaceHolder { get; set; } = "Email";
        
    public Login( IStringLocalizer stringLocalizer)
    {
        _stringLocalizer = stringLocalizer;
    }
    public async Task<ActionResult> OnGetAsync(string? returnUrl = null)
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            if (string.IsNullOrEmpty(returnUrl))
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            return Redirect(returnUrl!);
        }

        ReturnUrl = returnUrl ?? Url.Content("~/");
           
        Lang = CultureInfo.CurrentCulture.Name;

        UserNamePlaceHolder = _stringLocalizer[$"Login:Mode:userNameEmailPhone"] ;
        
        return Page();
    }

       
}