using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

[AllowAnonymous]
public class Login : PageModel
{
    private readonly IStringLocalizer _stringLocalizer;
    private readonly NuagesIdentityOptions _nuagesIdentityOptions;

    public string Lang { get; set; } = "en-CA";
    public string? ReturnUrl { get; set; }
    public string UserNamePlaceHolder { get; set; } = "Email";
        
    public Login( IStringLocalizer stringLocalizer, IOptions<NuagesIdentityOptions> nuagesIdentityOptions)
    {
        _stringLocalizer = stringLocalizer;
        _nuagesIdentityOptions = nuagesIdentityOptions.Value;
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

        UserNamePlaceHolder = _stringLocalizer[_nuagesIdentityOptions.SupportsUserName ? "Login:Mode:userNameEmail" : "Login:Mode:email"] ;
        
        return Page();
    }

       
}