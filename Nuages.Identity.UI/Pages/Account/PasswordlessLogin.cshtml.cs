using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.Login.Passwordless;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

public class PasswordlessLogin : PageModel
{
    private readonly IPasswordlessService _passwordlessService;

    public PasswordlessLogin(IPasswordlessService passwordlessService)
    {
        _passwordlessService = passwordlessService;
    }

    public virtual async Task<IActionResult> OnGet(string token, string userId)
    {
        var res = await _passwordlessService.LoginPasswordLess(token, userId);

        if (res.Success)
            return Redirect("/");

        if (res.Result.RequiresTwoFactor) return Redirect("/account/loginwith2fa?returnUrl=/");

        return Unauthorized();
    }
}