using System.Globalization;
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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizer _stringLocalizer;


    public string Lang { get; set; } = "en-CA";
    public string? ReturnUrl { get; set; }
    public string? RecaptchaSiteKey { get; set; }
    public string UserNamePlaceHolder { get; set; } = "Email";
        
    public Login( IStringLocalizer stringLocalizer, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _stringLocalizer = stringLocalizer;
    }
    public async Task<ActionResult> OnGetAsync(string? returnUrl = null)
    {
       
        // await _httpContextAccessor.HttpContext!.SignOutAsync(NuagesIdentityConstants.EmailNotVerifiedScheme);
        // await _httpContextAccessor.HttpContext!.SignOutAsync(NuagesIdentityConstants.PhoneNotVerifiedScheme);
        
        if (User.Identity is { IsAuthenticated: true })
        {
            //TODO : Logout si returnUrl n'est pas fourni
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(returnUrl!);
        }

        ReturnUrl = returnUrl ?? Url.Content("~/");
           
        Lang = CultureInfo.CurrentCulture.Name;

        RecaptchaSiteKey = "6Ldnbg4aAAAAAJhHymHUGQY9uqinHwIf7LYvRvpR";
        
        ViewData["LogoUrl"] = "";
        ViewData["ShowRegistration"] = false;
        ViewData["RegistrationUrl"] = "";

        UserNamePlaceHolder = _stringLocalizer[$"Login:Mode:userNameEmailPhone"] ;
        
        return Page();
    }

       
}