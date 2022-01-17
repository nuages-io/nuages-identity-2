using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.Passwordless;

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
        var res= await _passwordlessService.LoginPasswordLess(token, userId);

        if (res.Success)
            return Redirect("/");
        else
        {
            return Unauthorized();
        }
    }
}